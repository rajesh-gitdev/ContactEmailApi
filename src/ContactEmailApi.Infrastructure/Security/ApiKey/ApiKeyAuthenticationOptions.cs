using Microsoft.AspNetCore.Authentication;

namespace ContactEmailApi.Infrastructure.Security.ApiKey;

/// <summary>Options for the custom API-key authentication scheme (no extra settings today).</summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}
