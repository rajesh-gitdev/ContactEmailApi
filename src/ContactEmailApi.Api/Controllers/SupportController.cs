using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/support")]
[Authorize(Policy = Policies.WebsiteClients)]
[EnableRateLimiting(RateLimitPolicies.Contact)]
public sealed class SupportController(ISupportService supportService) : ApiControllerBase
{
    /// <summary>Creates a support ticket and notifies the support inbox.</summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SupportTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await supportService.CreateTicketAsync(request, RequestContext(), cancellationToken);
        return Success(result, $"Support ticket {result.TicketNumber} created.", StatusCodes.Status201Created);
    }
}
