using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>A business-to-business inquiry submitted via contact/business.</summary>
public sealed class BusinessInquiry : BaseEntity
{
    public string ReferenceCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public InquiryType InquiryType { get; set; } = InquiryType.General;
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional stated budget, stored as free text (e.g. "$10k-$25k").</summary>
    public string? EstimatedBudget { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
}
