using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>An audit record of every outbound email the system attempts to send.</summary>
public sealed class EmailLog : BaseEntity
{
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;

    public EmailTemplateType? TemplateType { get; set; }
    public EmailDeliveryStatus DeliveryStatus { get; set; } = EmailDeliveryStatus.Pending;

    public int AttemptCount { get; set; }
    public DateTimeOffset? SentAtUtc { get; set; }
    public string? ErrorMessage { get; set; }
}
