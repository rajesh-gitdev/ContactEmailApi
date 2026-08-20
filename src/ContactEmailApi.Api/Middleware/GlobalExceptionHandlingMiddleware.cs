using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Exceptions;
using ContactEmailApi.Shared.Models;

namespace ContactEmailApi.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and converts them into the standard
/// <see cref="ApiResponse"/> JSON envelope, mapping known application exceptions to their
/// intended status codes and logging everything else as a server error.
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var traceId = context.Items.TryGetValue(CustomHeaderNames.CorrelationId, out var cid)
            ? cid?.ToString()
            : context.TraceIdentifier;

        var (statusCode, message, errors) = exception switch
        {
            ValidationAppException v => (v.StatusCode, v.Message, v.Errors),
            AppException a => (a.StatusCode, a.Message, a.Errors),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", (IReadOnlyList<string>?)null)
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning("Handled application exception ({StatusCode}): {Message}. TraceId={TraceId}",
                statusCode, message, traceId);
        }

        var response = ApiResponse.Fail(message, statusCode, errors, traceId);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
