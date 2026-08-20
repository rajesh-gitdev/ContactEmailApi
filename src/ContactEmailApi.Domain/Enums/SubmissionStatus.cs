namespace ContactEmailApi.Domain.Enums;

/// <summary>Lifecycle state of an inbound form submission.</summary>
public enum SubmissionStatus
{
    Received = 0,
    Queued = 1,
    Sent = 2,
    Failed = 3
}
