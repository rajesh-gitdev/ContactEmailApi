using ContactEmailApi.Application.Models.Security;

namespace ContactEmailApi.Application.Abstractions.Security;

/// <summary>Validates an inbound API key against the configured key store.</summary>
public interface IApiKeyValidator
{
    /// <summary>
    /// Returns the matching <see cref="ApiKeyDescriptor"/> when the supplied key is
    /// valid and active; otherwise <c>null</c>.
    /// </summary>
    ApiKeyDescriptor? Validate(string? presentedKey);
}
