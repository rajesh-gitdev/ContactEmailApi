namespace ContactEmailApi.Application.Models.Email;

/// <summary>An email address with an optional display name.</summary>
public sealed record EmailAddress(string Address, string? DisplayName = null);
