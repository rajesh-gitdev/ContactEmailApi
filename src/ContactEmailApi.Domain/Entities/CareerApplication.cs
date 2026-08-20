using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>A job application submitted via career/apply.</summary>
public sealed class CareerApplication : BaseEntity
{
    public string ReferenceCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }

    /// <summary>Link to an externally hosted resume (uploads handled in a later phase).</summary>
    public string? ResumeUrl { get; set; }
    public string? LinkedInUrl { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
}
