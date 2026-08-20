namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/callback/request. (Suffixed "Dto" to avoid
/// clashing with the <c>CallbackRequest</c> domain entity.)</summary>
public sealed record CallbackRequestDto : ISpamProtectedRequest
{
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? PreferredTime { get; init; }
    public string? Reason { get; init; }

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
