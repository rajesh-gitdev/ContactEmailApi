using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Models.Email;
using ContactEmailApi.Domain.Enums;
using ContactEmailApi.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ContactEmailApi.Infrastructure.Email;

/// <summary>Sends email through the configured SMTP server using MailKit / MimeKit.</summary>
public sealed class MailKitEmailService : IEmailService
{
    private readonly IOptionsMonitor<SmtpOptions> _options;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IOptionsMonitor<SmtpOptions> options, ILogger<MailKitEmailService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtp = _options.CurrentValue;
        var mime = BuildMimeMessage(message, smtp);

        using var client = new SmtpClient();
        var socketOptions = smtp.UseSsl
            ? (smtp.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
            : SecureSocketOptions.None;

        await client.ConnectAsync(smtp.Host, smtp.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);
        }

        try
        {
            await client.SendAsync(mime, cancellationToken);
            _logger.LogInformation("Email sent to {Recipient} with subject {Subject}.",
                message.To.Address, message.Subject);
        }
        finally
        {
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
    }

    private static MimeMessage BuildMimeMessage(EmailMessage message, SmtpOptions smtp)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(smtp.SenderName, smtp.SenderEmail));
        mime.To.Add(new MailboxAddress(message.To.DisplayName ?? string.Empty, message.To.Address));

        foreach (var cc in message.Cc)
        {
            mime.Cc.Add(new MailboxAddress(cc.DisplayName ?? string.Empty, cc.Address));
        }

        foreach (var bcc in message.Bcc)
        {
            mime.Bcc.Add(new MailboxAddress(bcc.DisplayName ?? string.Empty, bcc.Address));
        }

        if (message.ReplyTo is not null)
        {
            mime.ReplyTo.Add(new MailboxAddress(message.ReplyTo.DisplayName ?? string.Empty, message.ReplyTo.Address));
        }

        mime.Subject = message.Subject;
        mime.Priority = message.Priority switch
        {
            EmailPriority.High => MessagePriority.Urgent,
            EmailPriority.Low => MessagePriority.NonUrgent,
            _ => MessagePriority.Normal
        };

        var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
        if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
        {
            builder.TextBody = message.PlainTextBody;
        }

        foreach (var attachment in message.Attachments)
        {
            builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        mime.Body = builder.ToMessageBody();
        return mime;
    }
}
