using ContactEmailApi.Application.Abstractions.Spam;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Models.Common;
using ContactEmailApi.Shared.Exceptions;

namespace ContactEmailApi.Infrastructure.Services.Common;

internal static class SpamGuardExtensions
{
    /// <summary>Evaluates the request and throws an <see cref="AppException"/> (HTTP 400)
    /// with a deliberately vague message when the submission is flagged as spam.</summary>
    public static async Task EnsureCleanAsync(
        this ISpamGuard guard,
        ISpamProtectedRequest request,
        string fingerprint,
        SubmissionContext context,
        CancellationToken cancellationToken)
    {
        var result = await guard.EvaluateAsync(request, fingerprint, context.IpAddress, cancellationToken);
        if (result.IsSpam)
        {
            // Vague on purpose: don't tell bots which check they tripped.
            throw new AppException("Your submission could not be processed. Please try again.", statusCode: 400);
        }
    }
}
