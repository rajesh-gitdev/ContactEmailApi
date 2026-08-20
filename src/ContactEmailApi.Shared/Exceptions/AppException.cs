namespace ContactEmailApi.Shared.Exceptions;

/// <summary>
/// Base class for expected application errors. The global exception middleware
/// maps these to their <see cref="StatusCode"/> instead of a generic 500.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public IReadOnlyList<string>? Errors { get; }

    public AppException(string message, int statusCode = 400, IReadOnlyList<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
