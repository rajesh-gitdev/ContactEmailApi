using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Models.Common;

namespace ContactEmailApi.Application.Abstractions.Spam;

/// <summary>
/// Central anti-spam gate applied to public form submissions. Layers a honeypot check,
/// a "filled too fast / too stale" timestamp check, in-memory duplicate detection, and
/// (optionally) reCAPTCHA verification.
/// </summary>
public interface ISpamGuard
{
    /// <param name="request">The submitted form (carries honeypot/timestamp/token).</param>
    /// <param name="fingerprint">Stable content fingerprint used for duplicate detection.</param>
    /// <param name="ipAddress">Caller IP, forwarded to reCAPTCHA verification.</param>
    Task<SpamCheckResult> EvaluateAsync(
        ISpamProtectedRequest request,
        string fingerprint,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
