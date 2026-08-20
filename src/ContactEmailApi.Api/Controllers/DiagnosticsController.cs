using ContactEmailApi.Application.Abstractions.Security;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

/// <summary>
/// Phase 1 diagnostics. Proves the pipeline end-to-end: routing, versioning, the
/// response envelope, authentication (JWT or API key) and rate limiting. The ten
/// business endpoints are added in Phase 3.
/// </summary>
[ApiController]
[Route("api/v1/diagnostics")]
[Produces("application/json")]
public sealed class DiagnosticsController : ControllerBase
{
    /// <summary>Anonymous liveness ping. Confirms the API is reachable.</summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        var traceId = HttpContext.Items[CustomHeaderNames.CorrelationId]?.ToString();
        return Ok(ApiResponse.Ok("pong", StatusCodes.Status200OK, requestId: Guid.NewGuid().ToString(), traceId: traceId));
    }

    /// <summary>
    /// Returns the authenticated caller's identity. Requires a valid JWT or API key and
    /// is throttled by the admin rate-limit policy.
    /// </summary>
    [HttpGet("whoami")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult WhoAmI([FromServices] ICurrentUser currentUser)
    {
        var traceId = HttpContext.Items[CustomHeaderNames.CorrelationId]?.ToString();

        var identity = new
        {
            currentUser.Subject,
            currentUser.AuthenticationScheme,
            currentUser.Roles
        };

        return Ok(ApiResponse<object>.Ok(identity, "Authenticated.", StatusCodes.Status200OK, traceId: traceId));
    }
}
