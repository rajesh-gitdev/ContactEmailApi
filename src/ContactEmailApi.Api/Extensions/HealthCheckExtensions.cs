using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContactEmailApi.Api.Extensions;

/// <summary>Maps liveness/readiness endpoints with a structured JSON writer.</summary>
public static class HealthCheckExtensions
{
    public static WebApplication MapApiHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteResponse });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteResponse
        });

        return app;
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
                error = e.Value.Exception?.Message
            }),
            timestamp = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        return context.Response.WriteAsync(json);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
