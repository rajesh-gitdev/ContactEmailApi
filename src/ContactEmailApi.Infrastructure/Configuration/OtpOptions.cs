namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>One-time-password settings, bound from the "Otp" section.</summary>
public sealed class OtpOptions
{
    public const string SectionName = "Otp";

    public int CodeLength { get; init; } = 6;
    public int ExpiryMinutes { get; init; } = 5;
}
