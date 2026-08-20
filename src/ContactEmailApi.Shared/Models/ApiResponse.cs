using System.Text.Json.Serialization;

namespace ContactEmailApi.Shared.Models;

/// <summary>
/// Standard response envelope returned by every endpoint (success or failure),
/// giving clients a single predictable shape to parse.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    /// <summary>Validation / business errors. Omitted from JSON when empty.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Errors { get; init; }

    /// <summary>Server-generated identifier for this specific request (a GUID).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RequestId { get; init; }

    public int StatusCode { get; init; }

    /// <summary>Correlation id, echoed back so clients can reference it in support tickets.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public static ApiResponse Ok(string message, int statusCode = 200, string? requestId = null, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        StatusCode = statusCode,
        RequestId = requestId,
        TraceId = traceId
    };

    public static ApiResponse Fail(string message, int statusCode, IReadOnlyList<string>? errors = null, string? traceId = null, string? requestId = null) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode,
        Errors = errors,
        TraceId = traceId,
        RequestId = requestId
    };
}

/// <summary>Response envelope that also carries a typed <typeparamref name="T"/> payload.</summary>
public sealed class ApiResponse<T> : ApiResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string message, int statusCode = 200, string? requestId = null, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        StatusCode = statusCode,
        RequestId = requestId,
        TraceId = traceId,
        Data = data
    };
}
