namespace ContactEmailApi.Domain.Enums;

/// <summary>Delivery outcome recorded for each outbound email.</summary>
public enum EmailDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
