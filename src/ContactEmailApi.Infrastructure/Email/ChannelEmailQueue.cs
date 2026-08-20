using System.Threading.Channels;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Models.Email;

namespace ContactEmailApi.Infrastructure.Email;

/// <summary>
/// A bounded in-process email queue backed by <see cref="System.Threading.Channels"/>.
/// Enqueue waits (back-pressure) rather than dropping messages when the queue is full.
/// </summary>
public sealed class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public ChannelEmailQueue(int capacity = 1000)
    {
        _channel = Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(message, cancellationToken);

    public ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAsync(cancellationToken);
}
