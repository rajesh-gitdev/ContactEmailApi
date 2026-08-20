using System.Net.Sockets;
using ContactEmailApi.Infrastructure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ContactEmailApi.Infrastructure.HealthChecks;

/// <summary>
/// Verifies the SMTP host is reachable via a lightweight TCP connect within a timeout.
/// (Full authenticated SMTP verification is performed by the email service in Phase 2.)
/// </summary>
public sealed class SmtpHealthCheck : IHealthCheck
{
    private readonly IOptionsMonitor<SmtpOptions> _options;

    public SmtpHealthCheck(IOptionsMonitor<SmtpOptions> options) => _options = options;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var smtp = _options.CurrentValue;

        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            return HealthCheckResult.Degraded("SMTP host is not configured.");
        }

        try
        {
            using var client = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(smtp.HealthCheckTimeoutMs);

            await client.ConnectAsync(smtp.Host, smtp.Port, timeoutCts.Token);

            return client.Connected
                ? HealthCheckResult.Healthy($"SMTP reachable at {smtp.Host}:{smtp.Port}.")
                : HealthCheckResult.Unhealthy($"Could not connect to {smtp.Host}:{smtp.Port}.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy($"SMTP connect to {smtp.Host}:{smtp.Port} timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SMTP connectivity check failed: {ex.Message}");
        }
    }
}
