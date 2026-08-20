namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>API keys accepted by the API, bound from the "ApiKeys" configuration section.</summary>
public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKeys";

    public List<ApiKeyEntry> Keys { get; init; } = [];
}

/// <summary>A single configured API key and the identity/role it grants.</summary>
public sealed class ApiKeyEntry
{
    /// <summary>The secret key value presented in the X-Api-Key header.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Human-readable owner (e.g. "MarketingSite").</summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>Role granted to callers using this key (Admin/Website/Internal/System).</summary>
    public string Role { get; init; } = string.Empty;

    public bool Enabled { get; init; } = true;
}
