namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/contact/send.</summary>
public sealed record ContactRequest : ISpamProtectedRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
