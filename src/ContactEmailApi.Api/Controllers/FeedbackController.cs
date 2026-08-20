using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/feedback")]
[Authorize(Policy = Policies.WebsiteClients)]
[EnableRateLimiting(RateLimitPolicies.Contact)]
public sealed class FeedbackController(IFeedbackService feedbackService) : ApiControllerBase
{
    /// <summary>Submits a star rating and feedback message.</summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Send([FromBody] FeedbackRequest request, CancellationToken cancellationToken)
    {
        await feedbackService.SubmitAsync(request, RequestContext(), cancellationToken);
        return Success("Thanks for your feedback.");
    }
}
