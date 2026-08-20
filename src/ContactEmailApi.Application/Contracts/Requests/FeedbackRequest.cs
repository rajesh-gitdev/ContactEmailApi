namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/feedback/send.</summary>
public sealed record FeedbackRequest : ISpamProtectedRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }

    /// <summary>1-5 star rating.</summary>
    public int Rating { get; init; }
    public string Message { get; init; } = string.Empty;

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
