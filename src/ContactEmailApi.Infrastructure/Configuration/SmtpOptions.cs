using System.ComponentModel.DataAnnotations;

namespace ContactEmailApi.Infrastructure.Configuration;

/// <summary>
/// SMTP settings bound from the "Smtp" section. Consumed fully by the MailKit-based
/// email service in Phase 2; used in Phase 1 by the SMTP health check.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public bool UseSsl { get; init; } = true;

    [Required, EmailAddress]
    public string SenderEmail { get; init; } = string.Empty;

    public string SenderName { get; init; } = string.Empty;

    /// <summary>Business inbox that contact messages are delivered to.</summary>
    [EmailAddress]
    public string BusinessEmail { get; init; } = string.Empty;

    /// <summary>Health-check TCP connect timeout in milliseconds.</summary>
    [Range(500, 30000)]
    public int HealthCheckTimeoutMs { get; init; } = 3000;
}
