namespace ContactEmailApi.Shared.Exceptions;

/// <summary>Raised when a requested resource does not exist. Maps to HTTP 404.</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, statusCode: 404)
    {
    }
}
