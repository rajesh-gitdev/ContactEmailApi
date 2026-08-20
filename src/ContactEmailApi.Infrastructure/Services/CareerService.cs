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

/// <summary>Handles career/apply.</summary>
public sealed class CareerService : ICareerService
{
    private readonly ApplicationDbContext _db;
    private readonly ISpamGuard _spamGuard;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IReferenceCodeGenerator _refGen;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<CareerService> _logger;

    public CareerService(
        ApplicationDbContext db, ISpamGuard spamGuard, IEmailQueue queue, IEmailTemplateRenderer renderer,
        IReferenceCodeGenerator refGen, IDateTimeProvider clock, IOptionsMonitor<SmtpOptions> smtp, ILogger<CareerService> logger)
    {
        _db = db; _spamGuard = spamGuard; _queue = queue; _renderer = renderer;
        _refGen = refGen; _clock = clock; _smtp = smtp.CurrentValue; _logger = logger;
    }

    public async Task<SubmissionAcceptedResponse> ApplyAsync(
        CareerApplicationRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var fingerprint = $"career|{request.Email}|{request.Position}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var entity = new CareerApplication
        {
            ReferenceCode = _refGen.Generate("JOB"),
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Position = request.Position,
            CoverLetter = request.CoverLetter,
            ResumeUrl = request.ResumeUrl,
            LinkedInUrl = request.LinkedInUrl,
            Status = SubmissionStatus.Queued
        };

        _db.CareerApplications.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["FullName"] = request.FullName;
        tokens["Email"] = request.Email;
        tokens["PhoneNumber"] = request.PhoneNumber;
        tokens["Position"] = request.Position;
        tokens["ResumeUrl"] = request.ResumeUrl ?? "-";
        tokens["LinkedInUrl"] = request.LinkedInUrl ?? "-";
        tokens["ReferenceCode"] = entity.ReferenceCode;
        tokens["CoverLetter"] = request.CoverLetter ?? "(no cover letter provided)";

        var rendered = _renderer.Render(EmailTemplateType.Career, $"Application: {request.Position} - {request.FullName}", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(_smtp.BusinessEmail),
            ReplyTo = new EmailAddress(request.Email, request.FullName),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TemplateType = EmailTemplateType.Career
        }, cancellationToken);

        _logger.LogInformation("Audit: career application {Reference} received for {Position}.", entity.ReferenceCode, request.Position);
        return new SubmissionAcceptedResponse(entity.ReferenceCode, entity.CreatedAtUtc);
    }
}
