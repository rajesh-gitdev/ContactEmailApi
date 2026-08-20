using System.Net;
using System.Security.Cryptography;
using System.Text;
using ContactEmailApi.Application.Abstractions.Common;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Abstractions.Services;
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

/// <summary>Handles email/send-otp, email/send, and email/test.</summary>
public sealed class EmailDispatchService : IEmailDispatchService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailQueue _queue;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IDateTimeProvider _clock;
    private readonly SmtpOptions _smtp;
    private readonly OtpOptions _otp;
    private readonly ILogger<EmailDispatchService> _logger;

    public EmailDispatchService(
        ApplicationDbContext db, IEmailQueue queue, IEmailTemplateRenderer renderer, IDateTimeProvider clock,
        IOptionsMonitor<SmtpOptions> smtp, IOptionsMonitor<OtpOptions> otp, ILogger<EmailDispatchService> logger)
    {
        _db = db; _queue = queue; _renderer = renderer; _clock = clock;
        _smtp = smtp.CurrentValue; _otp = otp.CurrentValue; _logger = logger;
    }

    public async Task<OtpIssuedResponse> SendOtpAsync(
        SendOtpRequest request, SubmissionContext context, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var code = GenerateNumericCode(_otp.CodeLength);
        var expiresAt = _clock.UtcNow.AddMinutes(_otp.ExpiryMinutes);

        _db.OtpCodes.Add(new OtpCode
        {
            Email = email,
            CodeHash = HashCode(code, email),
            Purpose = request.Purpose,
            ExpiresAtUtc = expiresAt
        });
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["Code"] = code;
        tokens["ExpiryMinutes"] = _otp.ExpiryMinutes.ToString();

        var rendered = _renderer.Render(EmailTemplateType.Otp, "Your verification code", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(email),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            Priority = EmailPriority.High,
            TemplateType = EmailTemplateType.Otp
        }, cancellationToken);

        // The code itself is never logged.
        _logger.LogInformation("Audit: OTP issued to {Email} for {Purpose}.", email, request.Purpose);
        return new OtpIssuedResponse(expiresAt, _otp.ExpiryMinutes * 60);
    }

    public async Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
    {
        // For plain-text sends, wrap the body in a minimal HTML shell and HTML-encode it.
        var htmlBody = request.IsHtml
            ? request.Body
            : $"<pre style=\"font-family:inherit;white-space:pre-wrap;\">{WebUtility.HtmlEncode(request.Body)}</pre>";

        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(request.To, request.ToDisplayName),
            Subject = request.Subject,
            HtmlBody = htmlBody,
            PlainTextBody = request.IsHtml ? null : request.Body
        }, cancellationToken);

        _logger.LogInformation("Audit: direct email queued to {Recipient}.", request.To);
    }

    public async Task SendTestAsync(TestEmailRequest request, CancellationToken cancellationToken = default)
    {
        var to = string.IsNullOrWhiteSpace(request.To) ? _smtp.BusinessEmail : request.To!;

        var tokens = EmailTokenBuilder.Create(_smtp.SenderName, _clock);
        tokens["Heading"] = "SMTP test email";
        tokens["Body"] = $"This is a test email confirming that {_smtp.SenderName} can deliver mail via {_smtp.Host}:{_smtp.Port}.";

        var rendered = _renderer.Render(EmailTemplateType.InternalNotification, "SMTP test email", tokens);
        await _queue.EnqueueAsync(new EmailMessage
        {
            To = new EmailAddress(to),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TemplateType = EmailTemplateType.InternalNotification
        }, cancellationToken);

        _logger.LogInformation("Audit: test email queued to {Recipient}.", to);
    }

    private static string GenerateNumericCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString().PadLeft(length, '0');
    }

    private static string HashCode(string code, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}"));
        return Convert.ToHexString(bytes);
    }
}
