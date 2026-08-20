namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/career/apply.</summary>
public sealed record CareerApplicationRequest : ISpamProtectedRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string? CoverLetter { get; init; }
    public string? ResumeUrl { get; init; }
    public string? LinkedInUrl { get; init; }

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
