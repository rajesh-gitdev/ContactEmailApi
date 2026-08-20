using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/email")]
[Authorize]
public sealed class EmailController(IEmailDispatchService dispatchService) : ApiControllerBase
{
    /// <summary>Issues a one-time password and emails it to the recipient.</summary>
    [HttpPost("send-otp")]
    [Authorize(Policy = Policies.WebsiteClients)]
    [EnableRateLimiting(RateLimitPolicies.Otp)]
    [ProducesResponseType(typeof(ApiResponse<OtpIssuedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatchService.SendOtpAsync(request, RequestContext(), cancellationToken);
        return Success(result, "A verification code has been sent.");
    }

    /// <summary>Sends an arbitrary email. Restricted to internal/admin callers.</summary>
    [HttpPost("send")]
    [Authorize(Policy = Policies.InternalClients)]
    [EnableRateLimiting(RateLimitPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Send([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
    {
        await dispatchService.SendAsync(request, cancellationToken);
        return Success("Email queued for delivery.", StatusCodes.Status202Accepted);
    }

    /// <summary>Sends a diagnostic test email. Admin only.</summary>
    [HttpPost("test")]
    [Authorize(Policy = Policies.AdminOnly)]
    [EnableRateLimiting(RateLimitPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Test([FromBody] TestEmailRequest request, CancellationToken cancellationToken)
    {
        await dispatchService.SendTestAsync(request, cancellationToken);
        return Success("Test email queued.", StatusCodes.Status202Accepted);
    }
}
