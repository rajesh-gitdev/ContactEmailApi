using System.Security.Cryptography;
using System.Text;
using ContactEmailApi.Application.Abstractions.Common;
using ContactEmailApi.Application.Abstractions.Spam;
using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Models.Common;
using ContactEmailApi.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.Spam;

/// <summary>
/// Default anti-spam gate. Order: honeypot, timestamp window, duplicate detection
/// (per-content fingerprint in the memory cache), then reCAPTCHA.
/// </summary>
public sealed class SpamGuard : ISpamGuard
{
    private readonly IOptionsMonitor<SpamProtectionOptions> _options;
    private readonly IMemoryCache _cache;
    private readonly IRecaptchaVerifier _recaptcha;
    private readonly IDateTimeProvider _clock;

    public SpamGuard(
        IOptionsMonitor<SpamProtectionOptions> options,
        IMemoryCache cache,
        IRecaptchaVerifier recaptcha,
        IDateTimeProvider clock)
    {
        _options = options;
        _cache = cache;
        _recaptcha = recaptcha;
        _clock = clock;
    }

    public async Task<SpamCheckResult> EvaluateAsync(
        ISpamProtectedRequest request,
        string fingerprint,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var options = _options.CurrentValue;
        if (!options.Enabled)
        {
            return SpamCheckResult.Clean();
        }

        // 1) Honeypot: hidden field must be empty.
        if (!string.IsNullOrWhiteSpace(request.Honeypot))
        {
            return SpamCheckResult.Spam("honeypot");
        }

        // 2) Timestamp window: reject too-fast (bot) or too-stale (recycled) forms.
        if (request.FormRenderedAtUnixMs is { } renderedMs)
        {
            var rendered = DateTimeOffset.FromUnixTimeMilliseconds(renderedMs);
            var age = _clock.UtcNow - rendered;

            if (age < TimeSpan.FromSeconds(options.MinFormFillSeconds))
            {
                return SpamCheckResult.Spam("submitted-too-fast");
            }

            if (age > TimeSpan.FromMinutes(options.MaxFormAgeMinutes))
            {
                return SpamCheckResult.Spam("form-expired");
            }
        }

        // 3) Duplicate detection: same content fingerprint within the window.
        var dedupeKey = "spam:dup:" + Hash(fingerprint);
        if (_cache.TryGetValue(dedupeKey, out _))
        {
            return SpamCheckResult.Spam("duplicate");
        }

        _cache.Set(dedupeKey, true, TimeSpan.FromSeconds(options.DuplicateWindowSeconds));

        // 4) reCAPTCHA (no-op when disabled).
        var recaptchaOk = await _recaptcha.VerifyAsync(request.RecaptchaToken, ipAddress, cancellationToken);
        if (!recaptchaOk)
        {
            return SpamCheckResult.Spam("recaptcha");
        }

        return SpamCheckResult.Clean();
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
