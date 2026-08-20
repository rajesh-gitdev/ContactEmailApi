namespace ContactEmailApi.Domain.Enums;

/// <summary>Bucket a support ticket belongs to.</summary>
public enum SupportCategory
{
    General = 0,
    Technical = 1,
    Billing = 2,
    Account = 3,
    FeatureRequest = 4,
    Other = 5
}
