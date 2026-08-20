namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/email/send (privileged direct send).</summary>
public sealed record SendEmailRequest
{
    public string To { get; init; } = string.Empty;
    public string? ToDisplayName { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;

    /// <summary>When true, <see cref="Body"/> is treated as HTML; otherwise plain text.</summary>
    public bool IsHtml { get; init; } = true;
}
