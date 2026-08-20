namespace ContactEmailApi.Application.Contracts.Requests;

/// <summary>
/// Implemented by public form requests that carry anti-spam fields. The honeypot must
/// stay empty; the timestamp powers the "filled too fast / too stale" check; the optional
/// reCAPTCHA token is verified server-side when reCAPTCHA is enabled.
/// </summary>
public interface ISpamProtectedRequest
{
    /// <summary>Hidden field; bots tend to fill it in, humans never see it.</summary>
    string? Honeypot { get; }

    /// <summary>Client timestamp (ms since epoch) when the form was rendered.</summary>
    long? FormRenderedAtUnixMs { get; }

    /// <summary>Google reCAPTCHA response token from the client, when reCAPTCHA is enabled.</summary>
    string? RecaptchaToken { get; }
}
