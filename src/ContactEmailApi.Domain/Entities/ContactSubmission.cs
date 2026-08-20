using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>A general "Send Message" contact-form submission.</summary>
public sealed class ContactSubmission : BaseEntity
{
    /// <summary>Human-friendly reference shown to the visitor (e.g. CT-8F3A21).</summary>
    public string ReferenceCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    /// <summary>Client IP captured for abuse/audit purposes.</summary>
    public string? IpAddress { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
}
