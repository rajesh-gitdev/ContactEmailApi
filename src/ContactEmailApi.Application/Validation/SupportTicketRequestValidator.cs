using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class SupportTicketRequestValidator : AbstractValidator<SupportTicketRequest>
{
    public SupportTicketRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).MaximumLength(8000);
        this.ApplySpamRules();
    }
}
