using ContactEmailApi.Domain.Common;

namespace ContactEmailApi.Domain.Entities;

/// <summary>A newsletter subscriber created via newsletter/subscribe.</summary>
public sealed class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }

    /// <summary>Whether the subscriber explicitly consented (GDPR/CAN-SPAM).</summary>
    public bool ConsentGiven { get; set; }

    public bool IsConfirmed { get; set; }
    public DateTimeOffset? ConfirmedAtUtc { get; set; }
    public DateTimeOffset? UnsubscribedAtUtc { get; set; }

    public bool IsActive => UnsubscribedAtUtc is null;
}
