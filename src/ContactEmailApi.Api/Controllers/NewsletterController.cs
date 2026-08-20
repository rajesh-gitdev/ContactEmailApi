using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ContactEmailApi.Api.Controllers;

[Route("api/v1/newsletter")]
[Authorize(Policy = Policies.WebsiteClients)]
[EnableRateLimiting(RateLimitPolicies.Newsletter)]
public sealed class NewsletterController(INewsletterService newsletterService) : ApiControllerBase
{
    /// <summary>Subscribes an email address to the newsletter (idempotent).</summary>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscriptionRequest request, CancellationToken cancellationToken)
    {
        await newsletterService.SubscribeAsync(request, RequestContext(), cancellationToken);
        return Success("You're subscribed. Please check your inbox.");
    }
}
