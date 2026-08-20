using ContactEmailApi.Application.Abstractions.Common;

namespace ContactEmailApi.Infrastructure.Services.Common;

/// <inheritdoc />
public sealed class ReferenceCodeGenerator : IReferenceCodeGenerator
{
    private readonly IDateTimeProvider _clock;

    public ReferenceCodeGenerator(IDateTimeProvider clock) => _clock = clock;

    public string Generate(string prefix)
        => $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    public string GenerateTicketNumber()
        => $"SUP-{_clock.UtcNow:yyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
}
