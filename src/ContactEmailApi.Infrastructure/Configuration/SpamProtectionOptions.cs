namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>Anti-spam tuning, bound from the "SpamProtection" section.</summary>
public sealed class SpamProtectionOptions
{
    public const string SectionName = "SpamProtection";

    public bool Enabled { get; init; } = true;

    /// <summary>Submissions completed faster than this are treated as bots.</summary>
    public int MinFormFillSeconds { get; init; } = 2;

    /// <summary>Forms older than this (stale token/tab) are rejected.</summary>
    public int MaxFormAgeMinutes { get; init; } = 120;

    /// <summary>Window in which an identical submission is treated as a duplicate.</summary>
    public int DuplicateWindowSeconds { get; init; } = 60;
}
