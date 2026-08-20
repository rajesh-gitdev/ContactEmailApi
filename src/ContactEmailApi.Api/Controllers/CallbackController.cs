using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/callback")]
[Authorize(Policy = Policies.WebsiteClients)]
[EnableRateLimiting(RateLimitPolicies.Contact)]
public sealed class CallbackController(ICallbackService callbackService) : ApiControllerBase
{
    /// <summary>Requests a callback from the business.</summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionAcceptedResponse>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RequestCallback([FromBody] CallbackRequestDto request, CancellationToken cancellationToken)
    {
        var result = await callbackService.RequestAsync(request, RequestContext(), cancellationToken);
        return Success(result, "We'll call you back shortly.", StatusCodes.Status202Accepted);
    }
}
