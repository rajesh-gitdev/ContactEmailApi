using ContactEmailApi.Application.Models.Common;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactEmailApi.Api.Controllers;

/// <summary>Shared helpers for building the standard response envelope and request context.</summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string? TraceId => HttpContext.Items[CustomHeaderNames.CorrelationId]?.ToString();

    /// <summary>Captures ambient request metadata for the service layer.</summary>
    protected SubmissionContext RequestContext() => new(
        IpAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent: Request.Headers.UserAgent.ToString());

    protected IActionResult Success(string message, int statusCode = StatusCodes.Status200OK)
        => StatusCode(statusCode, ApiResponse.Ok(message, statusCode, requestId: Guid.NewGuid().ToString(), traceId: TraceId));

    protected IActionResult Success<T>(T data, string message, int statusCode = StatusCodes.Status200OK)
        => StatusCode(statusCode, ApiResponse<T>.Ok(data, message, statusCode, requestId: Guid.NewGuid().ToString(), traceId: TraceId));
}
