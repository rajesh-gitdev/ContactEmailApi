namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>Google reCAPTCHA settings, bound from the "Recaptcha" section.</summary>
public sealed class RecaptchaOptions
{
    public const string SectionName = "Recaptcha";

    /// <summary>When false, verification is skipped and always succeeds.</summary>
    public bool Enabled { get; init; }

    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Minimum score to accept for reCAPTCHA v3 (0.0-1.0).</summary>
    public double MinimumScore { get; init; } = 0.5;

    public string VerifyUrl { get; init; } = "https://www.google.com/recaptcha/api/siteverify";
}
