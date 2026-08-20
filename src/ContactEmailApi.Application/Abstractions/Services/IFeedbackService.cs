using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Models.Common;

namespace ContactEmailApi.Application.Abstractions.Services;

public interface IFeedbackService
{
    Task SubmitAsync(FeedbackRequest request, SubmissionContext context, CancellationToken cancellationToken = default);
}
