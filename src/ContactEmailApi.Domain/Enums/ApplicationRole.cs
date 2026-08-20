namespace ContactEmailApi.Domain.Enums;

/// <summary>
/// Authorization roles supported by the API. Values are used both as JWT role
/// claims and as API-key role assignments.
/// </summary>
public enum ApplicationRole
{
    /// <summary>Full administrative access (e.g. SMTP test endpoint).</summary>
    Admin = 0,

    /// <summary>Public website front-ends calling contact/newsletter endpoints.</summary>
    Website = 1,

    /// <summary>Trusted internal back-office applications.</summary>
    Internal = 2,

    /// <summary>Machine-to-machine system integrations.</summary>
    System = 3
}
