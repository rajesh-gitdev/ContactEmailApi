namespace ContactEmailApi.Shared.Constants;

/// <summary>Authentication scheme names shared between Infrastructure and the API host.</summary>
public static class AuthSchemes
{
    /// <summary>Policy scheme that dispatches to JWT or API key based on request headers.</summary>
    public const string MultiAuth = "MultiAuth";

    /// <summary>Custom API-key scheme name.</summary>
    public const string ApiKey = "ApiKey";

    /// <summary>Header carrying the API key.</summary>
    public const string ApiKeyHeader = "X-Api-Key";
}

/// <summary>Role names. Must match <c>ContactEmailApi.Domain.Enums.ApplicationRole</c>.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Website = "Website";
    public const string Internal = "Internal";
    public const string System = "System";
}

/// <summary>Authorization policy names.</summary>
public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string WebsiteClients = "WebsiteClients";
    public const string InternalClients = "InternalClients";
    public const string SystemClients = "SystemClients";
}
