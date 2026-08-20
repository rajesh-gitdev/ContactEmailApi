namespace ContactEmailApi.Domain.Enums;

/// <summary>Delivery priority hint carried into the (Phase 2) email pipeline.</summary>
public enum EmailPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}
