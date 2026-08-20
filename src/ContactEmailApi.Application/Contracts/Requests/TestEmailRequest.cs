namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/email/test. When <see cref="To"/> is omitted the
/// configured business inbox is used.</summary>
public sealed record TestEmailRequest
{
    public string? To { get; init; }
}
