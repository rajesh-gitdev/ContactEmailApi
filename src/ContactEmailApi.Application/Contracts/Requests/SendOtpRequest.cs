using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>Payload for POST /api/v1/email/send-otp.</summary>
public sealed record SendOtpRequest
{
    public string Email { get; init; } = string.Empty;
    public OtpPurpose Purpose { get; init; } = OtpPurpose.Login;
}
