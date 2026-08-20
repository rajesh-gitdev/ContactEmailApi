using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Models.Common;

namespace ContactEmailApi.Application.Abstractions.Services;

public interface INewsletterService
{
    /// <summary>Subscribes (or re-activates) an address. Idempotent per email.</summary>
    Task SubscribeAsync(NewsletterSubscriptionRequest request, SubmissionContext context, CancellationToken cancellationToken = default);
}
