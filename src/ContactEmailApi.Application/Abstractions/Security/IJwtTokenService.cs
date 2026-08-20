using ContactEmailApi.Application.Models.Security;

namespace ContactEmailApi.Application.Abstractions.Security;

/// <summary>Issues signed JWT access tokens (and refresh-token placeholders).</summary>
public interface IJwtTokenService
{
    TokenResult CreateToken(TokenRequest request);
}
