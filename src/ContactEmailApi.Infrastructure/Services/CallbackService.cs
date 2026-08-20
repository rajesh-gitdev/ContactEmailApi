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

/// <summary>Handles callback/request.</summary>
public sealed class CallbackService : ICallbackService
{
    private readonly ApplicationDbContext _db;
    private readonly ISpamGuard _spamGuard;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IReferenceCodeGenerator _refGen;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<CallbackService> _logger;

    public CallbackService(
        ApplicationDbContext db, ISpamGuard spamGuard, IEmailQueue queue, IEmailTemplateRenderer renderer,
        IReferenceCodeGenerator refGen, IDateTimeProvider clock, IOptionsMonitor<SmtpOptions> smtp, ILogger<CallbackService> logger)
    {
        _db = db; _spamGuard = spamGuard; _queue = queue; _renderer = renderer;
        _refGen = refGen; _clock = clock; _smtp = smtp.CurrentValue; _logger = logger;
    }

    public async Task<SubmissionAcceptedResponse> RequestAsync(
        CallbackRequestDto request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var fingerprint = $"callback|{request.PhoneNumber}|{request.Name}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var entity = new CallbackRequest
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PreferredTime = request.PreferredTime,
            Reason = request.Reason,
            Status = SubmissionStatus.Queued
        };

        _db.CallbackRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var reference = _refGen.Generate("CB");
        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["Name"] = request.Name;
        tokens["PhoneNumber"] = request.PhoneNumber;
        tokens["PreferredTime"] = request.PreferredTime ?? "Any time";
        tokens["Reason"] = request.Reason ?? "(no reason provided)";

        var rendered = _renderer.Render(EmailTemplateType.Callback, $"Callback requested: {request.Name}", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(_smtp.BusinessEmail),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            Priority = EmailPriority.High,
            TemplateType = EmailTemplateType.Callback
        }, cancellationToken);

        _logger.LogInformation("Audit: callback requested by {Name}.", request.Name);
        return new SubmissionAcceptedResponse(reference, entity.CreatedAtUtc);
    }
}
