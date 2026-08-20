namespace ContactEmailApi.Application.Contracts.Responses;

/// <summary>Returned after an OTP is issued (never contains the code itself).</summary>
public sealed record OtpIssuedResponse(DateTimeOffset ExpiresAtUtc, int ValidForSeconds);
