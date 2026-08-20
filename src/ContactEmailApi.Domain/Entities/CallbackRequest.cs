using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>A "request a callback" entry created via callback/request.</summary>
public sealed class CallbackRequest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Free-text preferred window (e.g. "Weekdays 2-4pm ET").</summary>
    public string? PreferredTime { get; set; }
    public string? Reason { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
}
