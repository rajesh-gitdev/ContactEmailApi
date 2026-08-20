using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ContactEmailApi.Infrastructure.RateLimiting;

/// <summary>
/// Configures the built-in .NET rate limiter with per-IP fixed-window policies and a
/// consistent HTTP 429 JSON payload.
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        //services.AddRateLimiter(options =>
        //{
        //    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        //    AddFixedWindow(options, RateLimitPolicies.Contact, permitLimit: 5, window: TimeSpan.FromMinutes(1));
        //    AddFixedWindow(options, RateLimitPolicies.Newsletter, permitLimit: 3, window: TimeSpan.FromMinutes(1));
        //    AddFixedWindow(options, RateLimitPolicies.Otp, permitLimit: 3, window: TimeSpan.FromMinutes(5));
        //    AddFixedWindow(options, RateLimitPolicies.Admin, permitLimit: 60, window: TimeSpan.FromMinutes(1));

        //    options.OnRejected = async (context, cancellationToken) =>
        //    {
        //        var response = context.HttpContext.Response;
        //        response.ContentType = "application/json";

        //        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        //        {
        //            response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        //        }

        //        var traceId = context.HttpContext.Items.TryGetValue(CustomHeaderNames.CorrelationId, out var cid)
        //            ? cid?.ToString()
        //            : context.HttpContext.TraceIdentifier;

        //        var payload = ApiResponse.Fail(
        //            message: "Too many requests. Please slow down and try again shortly.",
        //            statusCode: StatusCodes.Status429TooManyRequests,
        //            traceId: traceId);

        //        await response.WriteAsJsonAsync(payload, JsonOptions, cancellationToken);
        //    };
        //});

        return services;
    }

    private static void AddFixedWindow(
        Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options,
        string policyName,
        int permitLimit,
        TimeSpan window)
    {
        options.AddPolicy(policyName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ResolveClientKey(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }));
    }

    // Partition by client IP. ForwardedHeaders middleware (configured in the API host)
    // ensures RemoteIpAddress reflects the real client behind a proxy/load balancer.
    private static string ResolveClientKey(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
