using ContactEmailApi.Application.Abstractions.Security;
using ContactEmailApi.Application.Models.Security;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

/// <summary>
/// Issues JWT access tokens to clients holding a valid API key. This lets a website
/// exchange its long-lived API key for a short-lived bearer token before calling the
/// business endpoints. (A full credential/refresh-token flow arrives in a later phase.)
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    /// <summary>Exchanges the presented X-Api-Key for a signed JWT bearer token.</summary>
    [HttpPost("token")]
    [Authorize(AuthenticationSchemes = AuthSchemes.ApiKey)]
    [EnableRateLimiting(RateLimitPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<TokenResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult IssueToken(
        [FromServices] IJwtTokenService tokenService,
        [FromServices] ICurrentUser currentUser)
    {
        var traceId = HttpContext.Items[CustomHeaderNames.CorrelationId]?.ToString();

        var token = tokenService.CreateToken(new TokenRequest(
            Subject: currentUser.Subject ?? "unknown",
            Roles: currentUser.Roles));

        return Ok(ApiResponse<TokenResult>.Ok(token, "Token issued.", StatusCodes.Status200OK, traceId: traceId));
    }
}
