using ContactEmailApi.Application.Models.Email;

namespace ContactEmailApi.Application.Abstractions.Email;

/// <summary>
/// In-process queue that decouples request handling from SMTP delivery. Producers
/// enqueue messages; the background processor drains and sends them.
/// </summary>
public interface IEmailQueue
{
    /// <summary>Enqueues a message for asynchronous delivery.</summary>
    ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>Awaits and removes the next queued message (used by the background processor).</summary>
    ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
}
