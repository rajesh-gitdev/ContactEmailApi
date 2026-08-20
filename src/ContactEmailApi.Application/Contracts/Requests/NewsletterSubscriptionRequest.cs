namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/newsletter/subscribe.</summary>
public sealed record NewsletterSubscriptionRequest : ISpamProtectedRequest
{
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }

    /// <summary>Must be true; the user must actively consent to receive the newsletter.</summary>
    public bool Consent { get; init; }

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
