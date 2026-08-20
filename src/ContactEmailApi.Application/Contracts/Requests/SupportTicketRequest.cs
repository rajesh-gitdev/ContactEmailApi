using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/support/create.</summary>
public sealed record SupportTicketRequest : ISpamProtectedRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public SupportCategory Category { get; init; } = SupportCategory.General;
    public SupportPriority Priority { get; init; } = SupportPriority.Normal;
    public string Subject { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
