namespace ContactEmailApi.Application.Abstractions.Spam;

/// <summary>Verifies a Google reCAPTCHA token. Returns true when reCAPTCHA is disabled.</summary>
public interface IRecaptchaVerifier
{
    Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default);
}
