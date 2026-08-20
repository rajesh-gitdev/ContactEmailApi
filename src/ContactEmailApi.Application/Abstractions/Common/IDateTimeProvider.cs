namespace ContactEmailApi.Application.Abstractions.Common;

/// <summary>Abstraction over the system clock to keep time-dependent logic testable.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
