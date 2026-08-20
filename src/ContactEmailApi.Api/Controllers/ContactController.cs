using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/contact")]
[Authorize(Policy = Policies.WebsiteClients)]
[EnableRateLimiting(RateLimitPolicies.Contact)]
public sealed class ContactController(IContactService contactService) : ApiControllerBase
{
    /// <summary>Sends a general contact message to the configured business inbox.</summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionAcceptedResponse>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Send([FromBody] ContactRequest request, CancellationToken cancellationToken)
    {
        var result = await contactService.SubmitContactAsync(request, RequestContext(), cancellationToken);
        return Success(result, "Your message has been received.", StatusCodes.Status202Accepted);
    }

    /// <summary>Submits a business-to-business inquiry.</summary>
    [HttpPost("business")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionAcceptedResponse>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Business([FromBody] BusinessInquiryRequest request, CancellationToken cancellationToken)
    {
        var result = await contactService.SubmitBusinessInquiryAsync(request, RequestContext(), cancellationToken);
        return Success(result, "Your inquiry has been received.", StatusCodes.Status202Accepted);
    }
}
