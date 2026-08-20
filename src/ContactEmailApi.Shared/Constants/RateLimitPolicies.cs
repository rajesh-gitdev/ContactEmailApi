namespace ContactEmailApi.Shared.Constants;

/// <summary>Named rate-limiting policies applied via <c>[EnableRateLimiting(...)]</c>.</summary>
public static class RateLimitPolicies
{
    public const string Contact = "contact";        // 5 / minute / IP
    public const string Newsletter = "newsletter";  // 3 / minute / IP
    public const string Otp = "otp";                // 3 / 5 minutes / IP
    public const string Admin = "admin";            // 60 / minute / IP
}
