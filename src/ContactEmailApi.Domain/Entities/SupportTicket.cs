using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>A support request created via support/create.</summary>
public sealed class SupportTicket : BaseEntity
{
    /// <summary>Sequential-style public ticket number (e.g. SUP-000123).</summary>
    public string TicketNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public SupportCategory Category { get; set; } = SupportCategory.General;
    public SupportPriority Priority { get; set; } = SupportPriority.Normal;

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
}
