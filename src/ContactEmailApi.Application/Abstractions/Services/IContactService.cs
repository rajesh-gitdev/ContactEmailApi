using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Application.Models.Common;

namespace ContactEmailApi.Application.Abstractions.Services;

public interface IContactService
{
    Task<SubmissionAcceptedResponse> SubmitContactAsync(ContactRequest request, SubmissionContext context, CancellationToken cancellationToken = default);
    Task<SubmissionAcceptedResponse> SubmitBusinessInquiryAsync(BusinessInquiryRequest request, SubmissionContext context, CancellationToken cancellationToken = default);
}
