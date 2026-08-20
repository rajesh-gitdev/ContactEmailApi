using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class BusinessInquiryRequestValidator : AbstractValidator<BusinessInquiryRequest>
{
    public BusinessInquiryRequestValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.PhoneNumber).MaximumLength(40).When(x => x.PhoneNumber is not null);
        RuleFor(x => x.InquiryType).IsInEnum();
        RuleFor(x => x.Message).NotEmpty().MinimumLength(10).MaximumLength(5000);
        RuleFor(x => x.EstimatedBudget).MaximumLength(100).When(x => x.EstimatedBudget is not null);
        this.ApplySpamRules();
    }
}
