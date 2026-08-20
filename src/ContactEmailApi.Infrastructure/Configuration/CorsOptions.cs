namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>Allowed cross-origin settings, bound from the "Cors" configuration section.</summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>Explicit list of allowed origins. Wildcards are rejected in Production.</summary>
    public string[] AllowedOrigins { get; init; } = [];
}
