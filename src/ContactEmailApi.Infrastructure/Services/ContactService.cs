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

/// <summary>Handles contact/send and contact/business submissions.</summary>
public sealed class ContactService : IContactService
{
    private readonly ApplicationDbContext _db;
    private readonly ISpamGuard _spamGuard;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IReferenceCodeGenerator _refGen;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        ApplicationDbContext db,
        ISpamGuard spamGuard,
        IEmailQueue queue,
        IEmailTemplateRenderer renderer,
        IReferenceCodeGenerator refGen,
        IDateTimeProvider clock,
        IOptionsMonitor<SmtpOptions> smtp,
        ILogger<ContactService> logger)
    {
        _db = db;
        _spamGuard = spamGuard;
        _queue = queue;
        _renderer = renderer;
        _refGen = refGen;
        _clock = clock;
        _smtp = smtp.CurrentValue;
        _logger = logger;
    }

    public async Task<SubmissionAcceptedResponse> SubmitContactAsync(
        ContactRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var fingerprint = $"contact|{request.Email}|{request.Subject}|{request.Message}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var entity = new ContactSubmission
        {
            ReferenceCode = _refGen.Generate("CT"),
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            PhoneNumber = request.PhoneNumber,
            IpAddress = context.IpAddress,
            Status = SubmissionStatus.Queued
        };

        _db.ContactSubmissions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["Name"] = request.Name;
        tokens["Email"] = request.Email;
        tokens["PhoneNumber"] = request.PhoneNumber ?? "-";
        tokens["Subject"] = request.Subject;
        tokens["ReferenceCode"] = entity.ReferenceCode;
        tokens["Message"] = request.Message;

        var rendered = _renderer.Render(EmailTemplateType.Contact, $"New contact message: {request.Subject}", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(_smtp.BusinessEmail),
            ReplyTo = new EmailAddress(request.Email, request.Name),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TemplateType = EmailTemplateType.Contact
        }, cancellationToken);

        _logger.LogInformation("Audit: contact submission {Reference} accepted from {Email}.",
            entity.ReferenceCode, request.Email);

        return new SubmissionAcceptedResponse(entity.ReferenceCode, entity.CreatedAtUtc);
    }

    public async Task<SubmissionAcceptedResponse> SubmitBusinessInquiryAsync(
        BusinessInquiryRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var fingerprint = $"business|{request.Email}|{request.CompanyName}|{request.Message}";
        await _spamGuard.EnsureCleanAsync(request, fingerprint, context, cancellationToken);

        var entity = new BusinessInquiry
        {
            ReferenceCode = _refGen.Generate("BIZ"),
            CompanyName = request.CompanyName,
            ContactName = request.ContactName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            InquiryType = request.InquiryType,
            Message = request.Message,
            EstimatedBudget = request.EstimatedBudget,
            Status = SubmissionStatus.Queued
        };

        _db.BusinessInquiries.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["CompanyName"] = request.CompanyName;
        tokens["ContactName"] = request.ContactName;
        tokens["Email"] = request.Email;
        tokens["PhoneNumber"] = request.PhoneNumber ?? "-";
        tokens["InquiryType"] = request.InquiryType.ToString();
        tokens["EstimatedBudget"] = request.EstimatedBudget ?? "-";
        tokens["ReferenceCode"] = entity.ReferenceCode;
        tokens["Message"] = request.Message;

        var rendered = _renderer.Render(EmailTemplateType.BusinessInquiry, $"New business inquiry: {request.CompanyName}", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(_smtp.BusinessEmail),
            ReplyTo = new EmailAddress(request.Email, request.ContactName),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            Priority = EmailPriority.High,
            TemplateType = EmailTemplateType.BusinessInquiry
        }, cancellationToken);

        _logger.LogInformation("Audit: business inquiry {Reference} accepted from {Company}.",
            entity.ReferenceCode, request.CompanyName);

        return new SubmissionAcceptedResponse(entity.ReferenceCode, entity.CreatedAtUtc);
    }
}
