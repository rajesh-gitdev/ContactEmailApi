using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>User feedback submitted via feedback/send.</summary>
public sealed class FeedbackEntry : BaseEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }

    /// <summary>1-5 star rating.</summary>
    public int Rating { get; set; }

    public string Message { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
}
