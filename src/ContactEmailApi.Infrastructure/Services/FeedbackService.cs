using ContactEmailApi.Application.Abstractions.Common;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Abstractions.Spam;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Models.Common;
using ContactEmailApi.Application.Models.Email;
using ContactEmailApi.Domain.Entities;
using ContactEmailApi.Domain.Enums;
using ContactEmailApi.Infrastructure.Configuration;
using ContactEmailApi.Infrastructure.Services.Common;
using ContactEmailApi.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.Services;

/// <summary>Handles feedback/send.</summary>
public sealed class FeedbackService : IFeedbackService
{
    private readonly ApplicationDbContext _db;
    private readonly ISpamGuard _spamGuard;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        ApplicationDbContext db, ISpamGuard spamGuard, IEmailQueue queue, IEmailTemplateRenderer renderer,
        IDateTimeProvider clock, IOptionsMonitor<SmtpOptions> smtp, ILogger<FeedbackService> logger)
    {
        _db = db; _spamGuard = spamGuard; _queue = queue; _renderer = renderer;
        _clock = clock; _smtp = smtp.CurrentValue; _logger = logger;
    }

    public async Task SubmitAsync(
        FeedbackRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var fingerprint = $"feedback|{request.Email}|{request.Rating}|{request.Message}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var entity = new FeedbackEntry
        {
            Name = request.Name,
            Email = request.Email,
            Rating = request.Rating,
            Message = request.Message,
            Status = SubmissionStatus.Queued
        };

        _db.FeedbackEntries.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["Rating"] = request.Rating.ToString();
        tokens["RatingStars"] = new string('*', Math.Clamp(request.Rating, 0, 5));
        tokens["Name"] = request.Name ?? "Anonymous";
        tokens["Email"] = request.Email ?? "-";
        tokens["Message"] = request.Message;

        var rendered = _renderer.Render(EmailTemplateType.Feedback, $"New feedback ({request.Rating}/5)", tokens);

        var message = new EmailMessage
        {
            To = new EmailAddress(_smtp.BusinessEmail),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TemplateType = EmailTemplateType.Feedback,
            ReplyTo = string.IsNullOrWhiteSpace(request.Email) ? null : new EmailAddress(request.Email!, request.Name)
        };
        await _queue.EnqueueAsync(message, cancellationToken);

        _logger.LogInformation("Audit: feedback received (rating {Rating}).", request.Rating);
    }
}
