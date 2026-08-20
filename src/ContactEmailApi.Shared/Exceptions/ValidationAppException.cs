namespace ContactEmailApi.Shared.Exceptions;

/// <summary>Raised when input validation fails. Maps to HTTP 422.</summary>
public sealed class ValidationAppException : AppException
{
    public ValidationAppException(IReadOnlyList<string> errors)
        : base("Validation failed.", statusCode: 422, errors: errors)
    {
    }
}
