using ContactEmailApi.Application.Models.Email;

namespace ContactEmailApi.Application.Abstractions.Email;

/// <summary>Sends an <see cref="EmailMessage"/> synchronously via the configured SMTP server.</summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
