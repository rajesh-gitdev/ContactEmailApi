namespace ContactEmailApi.Application.Models.Security;

/// <summary>A validated API key together with the identity it represents.</summary>
public sealed record ApiKeyDescriptor(string Owner, string Role);
