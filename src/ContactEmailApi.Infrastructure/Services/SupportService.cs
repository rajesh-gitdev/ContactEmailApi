using ContactEmailApi.Application.Abstractions.Common;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Abstractions.Spam;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
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

/// <summary>Handles support/create.</summary>
public sealed class SupportService : ISupportService
{
    private readonly ApplicationDbContext _db;
    private readonly ISpamGuard _spamGuard;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IReferenceCodeGenerator _refGen;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<SupportService> _logger;

    public SupportService(
        ApplicationDbContext db, ISpamGuard spamGuard, IEmailQueue queue, IEmailTemplateRenderer renderer,
        IReferenceCodeGenerator refGen, IDateTimeProvider clock, IOptionsMonitor<SmtpOptions> smtp, ILogger<SupportService> logger)
    {
        _db = db; _spamGuard = spamGuard; _queue = queue; _renderer = renderer;
        _refGen = refGen; _clock = clock; _smtp = smtp.CurrentValue; _logger = logger;
    }

    public async Task<SupportTicketResponse> CreateTicketAsync(
        SupportTicketRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var fingerprint = $"support|{request.Email}|{request.Subject}|{request.Description}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var entity = new SupportTicket
        {
            TicketNumber = _refGen.GenerateTicketNumber(),
            Name = request.Name,
            Email = request.Email,
            Category = request.Category,
            Priority = request.Priority,
            Subject = request.Subject,
            Description = request.Description,
            Status = SubmissionStatus.Queued
        };

        _db.SupportTickets.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["TicketNumber"] = entity.TicketNumber;
        tokens["Name"] = request.Name;
        tokens["Email"] = request.Email;
        tokens["Category"] = request.Category.ToString();
        tokens["Priority"] = request.Priority.ToString();
        tokens["Subject"] = request.Subject;
        tokens["Description"] = request.Description;

        var priority = request.Priority == SupportPriority.Urgent ? EmailPriority.High : EmailPriority.Normal;
        var rendered = _renderer.Render(EmailTemplateType.Support, $"[{entity.TicketNumber}] {request.Subject}", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(_smtp.BusinessEmail),
            ReplyTo = new EmailAddress(request.Email, request.Name),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            Priority = priority,
            TemplateType = EmailTemplateType.Support
        }, cancellationToken);

        _logger.LogInformation("Audit: support ticket {Ticket} created for {Email}.", entity.TicketNumber, request.Email);
        return new SupportTicketResponse(entity.TicketNumber, entity.CreatedAtUtc);
    }
}
