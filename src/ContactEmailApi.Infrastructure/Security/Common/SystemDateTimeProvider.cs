using ContactEmailApi.Application.Abstractions.Common;

namespace ContactEmailApi.Infrastructure.Security.Common;

/// <inheritdoc />
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
