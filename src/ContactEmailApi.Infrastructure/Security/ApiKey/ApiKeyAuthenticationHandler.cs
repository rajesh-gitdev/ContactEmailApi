using System.Security.Claims;
using System.Text.Encodings.Web;
using ContactEmailApi.Application.Abstractions.Security;
using ContactEmailApi.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.Security.ApiKey;

/// <summary>
/// Authenticates requests presenting a valid <c>X-Api-Key</c> header. On success it
/// materialises a <see cref="ClaimsPrincipal"/> with the role bound to that key.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyValidator _validator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidator validator)
        : base(options, logger, encoder)
    {
        _validator = validator;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthSchemes.ApiKeyHeader, out var headerValues))
        {
            // No API key on this request: let other schemes handle it.
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var presentedKey = headerValues.ToString();
        var descriptor = _validator.Validate(presentedKey);

        if (descriptor is null)
        {
            Logger.LogWarning("Rejected request with invalid API key from {RemoteIp}.",
                Context.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, descriptor.Owner),
            new Claim(ClaimTypes.Name, descriptor.Owner),
            new Claim(ClaimTypes.Role, descriptor.Role),
            new Claim("auth_method", AuthSchemes.ApiKey)
        };

        var identity = new ClaimsIdentity(claims, AuthSchemes.ApiKey);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthSchemes.ApiKey);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
