namespace ContactEmailApi.Application.Abstractions.Common;

/// <summary>Generates human-friendly reference and ticket identifiers.</summary>
public interface IReferenceCodeGenerator
{
    /// <summary>Returns a prefixed reference such as CT-9F3A21C4.</summary>
    string Generate(string prefix);

    /// <summary>Returns a support ticket number such as SUP-260704-8F3A.</summary>
    string GenerateTicketNumber();
}
