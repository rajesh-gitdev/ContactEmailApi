using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ContactEmailApi.Application.Abstractions.Spam;
using ContactEmailApi.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.Spam;

/// <summary>Verifies reCAPTCHA tokens against Google's siteverify endpoint.</summary>
public sealed class GoogleRecaptchaVerifier : IRecaptchaVerifier
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<RecaptchaOptions> _options;
    private readonly ILogger<GoogleRecaptchaVerifier> _logger;

    public GoogleRecaptchaVerifier(
        HttpClient httpClient,
        IOptionsMonitor<RecaptchaOptions> options,
        ILogger<GoogleRecaptchaVerifier> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        var options = _options.CurrentValue;
        if (!options.Enabled)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new("secret", options.SecretKey),
                new("response", token)
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                form.Add(new("remoteip", remoteIp));
            }

            using var content = new FormUrlEncodedContent(form);
            using var response = await _httpClient.PostAsync(options.VerifyUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken);
            if (result is null)
            {
                return false;
            }

            // v3 returns a score; v2 does not (Score == 0). Accept when success and, if a
            // score is present, it meets the configured threshold.
            return result.Success && (result.Score is null || result.Score >= options.MinimumScore);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "reCAPTCHA verification failed to complete; treating as unverified.");
            return false;
        }
    }

    private sealed record RecaptchaResponse
    {
        [JsonPropertyName("success")] public bool Success { get; init; }
        [JsonPropertyName("score")] public double? Score { get; init; }
        [JsonPropertyName("action")] public string? Action { get; init; }
    }
}
