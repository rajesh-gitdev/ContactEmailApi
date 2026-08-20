namespace ContactEmailApi.Application.Models.Common;

/// <summary>Outcome of an anti-spam evaluation.</summary>
public sealed record SpamCheckResult(bool IsSpam, string? Reason)
{
    public static SpamCheckResult Clean() => new(false, null);
    public static SpamCheckResult Spam(string reason) => new(true, reason);
}
