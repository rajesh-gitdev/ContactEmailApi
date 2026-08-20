using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ContactEmailApi.Application.Abstractions.Common;
using ContactEmailApi.Application.Abstractions.Security;
using ContactEmailApi.Application.Models.Security;
using ContactEmailApi.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ContactEmailApi.Infrastructure.Security.Jwt;

/// <inheritdoc />
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public TokenResult CreateToken(TokenRequest request)
    {
        var now = _clock.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(request.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        var accessToken = handler.CreateToken(descriptor);

        return new TokenResult(
            AccessToken: accessToken,
            ExpiresAtUtc: expires,
            TokenType: "Bearer",
            RefreshToken: GenerateRefreshToken());
    }

    // Cryptographically-strong opaque refresh token. Persistence/rotation lands in a later phase.
    private static string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
