using ContactEmailApi.Infrastructure.Configuration;
using ContactEmailApi.Infrastructure.Security.ApiKey;
using Microsoft.Extensions.Options;
using Xunit;

namespace ContactEmailApi.UnitTests;

public sealed class ApiKeyValidatorTests
{
    private static ApiKeyValidator CreateValidator(params ApiKeyEntry[] entries)
    {
        var options = new ApiKeyOptions { Keys = [.. entries] };
        var monitor = new StaticOptionsMonitor<ApiKeyOptions>(options);
        return new ApiKeyValidator(monitor);
    }

    [Fact]
    public void Validate_ReturnsDescriptor_ForValidEnabledKey()
    {
        var validator = CreateValidator(new ApiKeyEntry
        {
            Key = "secret-key-123",
            Owner = "Website",
            Role = "Website",
            Enabled = true
        });

        var result = validator.Validate("secret-key-123");

        Assert.NotNull(result);
        Assert.Equal("Website", result!.Owner);
        Assert.Equal("Website", result.Role);
    }

    [Fact]
    public void Validate_ReturnsNull_ForDisabledKey()
    {
        var validator = CreateValidator(new ApiKeyEntry
        {
            Key = "secret-key-123",
            Owner = "Website",
            Role = "Website",
            Enabled = false
        });

        Assert.Null(validator.Validate("secret-key-123"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("wrong-key")]
    public void Validate_ReturnsNull_ForMissingOrUnknownKey(string? presented)
    {
        var validator = CreateValidator(new ApiKeyEntry
        {
            Key = "secret-key-123",
            Owner = "Website",
            Role = "Website",
            Enabled = true
        });

        Assert.Null(validator.Validate(presented));
    }

    // Minimal IOptionsMonitor test double.
    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = value;
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
