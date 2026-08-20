using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Contracts.Responses;
using ContactEmailApi.Application.Models.Common;

namespace ContactEmailApi.Application.Abstractions.Services;

/// <summary>Handles the direct email endpoints: OTP issuance, privileged send, and test.</summary>
public interface IEmailDispatchService
{
    Task<OtpIssuedResponse> SendOtpAsync(SendOtpRequest request, SubmissionContext context, CancellationToken cancellationToken = default);
    Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
    Task SendTestAsync(TestEmailRequest request, CancellationToken cancellationToken = default);
}
