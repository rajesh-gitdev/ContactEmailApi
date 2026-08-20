namespace ContactEmailApi.Domain.Common;

/// <summary>
/// Base type for all persisted domain entities. Provides identity and audit fields
/// that the persistence layer populates automatically.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
