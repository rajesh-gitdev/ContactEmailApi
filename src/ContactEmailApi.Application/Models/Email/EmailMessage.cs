using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Application.Models.Email;

/// <summary>
/// A provider-agnostic outbound email. The Infrastructure email service translates
/// this into a MailKit/MimeKit message.
/// </summary>
public sealed class EmailMessage
{
    public required EmailAddress To { get; init; }

    /// <summary>Address the recipient should reply to (e.g. the form submitter).</summary>
    public EmailAddress? ReplyTo { get; init; }

    public IReadOnlyList<EmailAddress> Cc { get; init; } = [];
    public IReadOnlyList<EmailAddress> Bcc { get; init; } = [];

    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }

    /// <summary>Optional plain-text alternative; auto-derived from HTML when omitted.</summary>
    public string? PlainTextBody { get; init; }

    public EmailPriority Priority { get; init; } = EmailPriority.Normal;

    /// <summary>Template this message was rendered from (recorded in the email log).</summary>
    public EmailTemplateType? TemplateType { get; init; }

    public IReadOnlyList<EmailAttachment> Attachments { get; init; } = [];
}
