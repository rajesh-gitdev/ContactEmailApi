namespace ContactEmailApi.Application.Models.Security;

/// <summary>The result of issuing a token pair.</summary>
public sealed record TokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string TokenType,
    string RefreshToken);
