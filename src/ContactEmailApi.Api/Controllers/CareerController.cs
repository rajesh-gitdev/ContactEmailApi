using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/career")]
[Authorize(Policy = Policies.WebsiteClients)]
[EnableRateLimiting(RateLimitPolicies.Contact)]
public sealed class CareerController(ICareerService careerService) : ApiControllerBase
{
    /// <summary>Submits a job application.</summary>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionAcceptedResponse>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Apply([FromBody] CareerApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await careerService.ApplyAsync(request, RequestContext(), cancellationToken);
        return Success(result, "Your application has been received.", StatusCodes.Status202Accepted);
    }
}
