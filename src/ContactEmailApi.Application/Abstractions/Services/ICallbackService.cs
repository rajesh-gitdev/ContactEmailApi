using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Application.Models.Common;

namespace ContactEmailApi.Application.Abstractions.Services;

public interface ICallbackService
{
    Task<SubmissionAcceptedResponse> RequestAsync(CallbackRequestDto request, SubmissionContext context, CancellationToken cancellationToken = default);
}
