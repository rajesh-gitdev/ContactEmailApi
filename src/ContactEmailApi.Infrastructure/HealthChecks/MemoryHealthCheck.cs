using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContactEmailApi.Infrastructure.HealthChecks;

/// <summary>Reports Degraded when managed memory exceeds a configured threshold.</summary>
public sealed class MemoryHealthCheck : IHealthCheck
{
    private const long ThresholdBytes = 1024L * 1024L * 1024L; // 1 GB

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var data = new Dictionary<string, object>
        {
            ["AllocatedBytes"] = allocated,
            ["ThresholdBytes"] = ThresholdBytes,
            ["Gen0Collections"] = GC.CollectionCount(0),
            ["Gen1Collections"] = GC.CollectionCount(1),
            ["Gen2Collections"] = GC.CollectionCount(2)
        };

        var status = allocated < ThresholdBytes ? HealthStatus.Healthy : HealthStatus.Degraded;
        var description = $"Managed memory {allocated / (1024 * 1024)} MB (threshold {ThresholdBytes / (1024 * 1024)} MB).";

        return Task.FromResult(new HealthCheckResult(status, description, data: data));
    }
}
