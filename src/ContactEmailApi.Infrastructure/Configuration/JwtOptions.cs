using System.ComponentModel.DataAnnotations;

namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>Strongly-typed JWT settings bound from the "Jwt" configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>Symmetric signing key. Must be at least 32 bytes (256 bits) for HS256.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; init; } = 30;

    [Range(1, 365)]
    public int RefreshTokenDays { get; init; } = 7;

    /// <summary>Clock-skew tolerance (seconds) applied during token validation.</summary>
    [Range(0, 300)]
    public int ClockSkewSeconds { get; init; } = 30;
}
