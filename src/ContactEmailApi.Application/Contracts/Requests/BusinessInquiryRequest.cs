using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/contact/business.</summary>
public sealed record BusinessInquiryRequest : ISpamProtectedRequest
{
    public string CompanyName { get; init; } = string.Empty;
    public string ContactName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public InquiryType InquiryType { get; init; } = InquiryType.General;
    public string Message { get; init; } = string.Empty;
    public string? EstimatedBudget { get; init; }

    public string? Honeypot { get; init; }
    public long? FormRenderedAtUnixMs { get; init; }
    public string? RecaptchaToken { get; init; }
}
