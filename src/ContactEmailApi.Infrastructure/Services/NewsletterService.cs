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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.Services;

/// <summary>Handles newsletter/subscribe. Idempotent per email address.</summary>
public sealed class NewsletterService : INewsletterService
{
    private readonly ApplicationDbContext _db;
    private readonly ISpamGuard _spamGuard;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        ApplicationDbContext db, ISpamGuard spamGuard, IEmailQueue queue, IEmailTemplateRenderer renderer,
        IDateTimeProvider clock, IOptionsMonitor<SmtpOptions> smtp, ILogger<NewsletterService> logger)
    {
        _db = db; _spamGuard = spamGuard; _queue = queue; _renderer = renderer;
        _clock = clock; _smtp = smtp.CurrentValue; _logger = logger;
    }

    public async Task SubscribeAsync(
        NewsletterSubscriptionRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var fingerprint = $"newsletter|{email}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var existing = await _db.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);

        var isNew = existing is null;
        if (existing is null)
        {
            existing = new NewsletterSubscriber { Email = email };
            _db.NewsletterSubscribers.Add(existing);
        }

        existing.Name = request.Name ?? existing.Name;
        existing.ConsentGiven = request.Consent;
        existing.UnsubscribedAtUtc = null; // (re)activate

        await _db.SaveChangesAsync(cancellationToken);

        // Only welcome brand-new subscribers to avoid re-welcoming re-subscribers.
        if (isNew)
        {
            var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
            tokens["Name"] = request.Name ?? "there";
            tokens["UnsubscribeUrl"] = "#"; // replaced with a real link when the unsubscribe flow ships

            var rendered = _renderer.Render(EmailTemplateType.Welcome, $"Welcome to {_smtp.SenderName}", tokens);
            await _queue.EnqueueAsync(new EmailMessage
            {
                To = new EmailAddress(email, request.Name),
                Subject = rendered.Subject,
                HtmlBody = rendered.HtmlBody,
                TemplateType = EmailTemplateType.Welcome
            }, cancellationToken);
        }

        _logger.LogInformation("Audit: newsletter subscription for {Email} (new={IsNew}).", email, isNew);
    }
}
