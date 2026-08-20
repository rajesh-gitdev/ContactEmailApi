using System.Security.Cryptography;
using System.Text;
using ContactEmailApi.Application.Abstractions.Security;
using ContactEmailApi.Application.Models.Security;
using ContactEmailApi.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.Security.ApiKey;

/// <inheritdoc />
public sealed class ApiKeyValidator : IApiKeyValidator
{
    private readonly IOptionsMonitor<ApiKeyOptions> _options;

    public ApiKeyValidator(IOptionsMonitor<ApiKeyOptions> options) => _options = options;

    public ApiKeyDescriptor? Validate(string? presentedKey)
    {
        if (string.IsNullOrWhiteSpace(presentedKey))
        {
            return null;
        }

        foreach (var entry in _options.CurrentValue.Keys)
        {
            if (entry.Enabled && FixedTimeEquals(entry.Key, presentedKey))
            {
                return new ApiKeyDescriptor(entry.Owner, entry.Role);
            }
        }

        return null;
    }

    // Constant-time comparison to avoid leaking key length/content via timing.
    private static bool FixedTimeEquals(string a, string b)
    {
        var ab = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(ab, bb);
    }
}
