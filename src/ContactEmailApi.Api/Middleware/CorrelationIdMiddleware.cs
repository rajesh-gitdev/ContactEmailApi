using ContactEmailApi.Shared.Constants;
using Serilog.Context;

namespace ContactEmailApi.Api.Middleware;

/// <summary>
/// Ensures every request has a correlation id: reads an inbound X-Correlation-ID or
/// generates one, stores it for downstream use, echoes it on the response, and pushes
/// it into the Serilog LogContext so it appears on every log line for the request.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CustomHeaderNames.CorrelationId, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : Guid.NewGuid().ToString("N");

        context.Items[CustomHeaderNames.CorrelationId] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CustomHeaderNames.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
