namespace ContactEmailApi.Application.Abstractions.Security;

/// <summary>Ambient accessor for the authenticated caller of the current request.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? Subject { get; }

    string? AuthenticationScheme { get; }

    IReadOnlyList<string> Roles { get; }
}
