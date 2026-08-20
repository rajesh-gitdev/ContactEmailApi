namespace ContactEmailApi.Application.Models.Security;

/// <summary>Inputs required to mint a JWT access token.</summary>
public sealed record TokenRequest(string Subject, IReadOnlyList<string> Roles);
