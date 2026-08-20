using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class CallbackRequestValidator : AbstractValidator<CallbackRequestDto>
{
    public CallbackRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(40)
            .Matches(@"^[0-9+()\-\s]+$").WithMessage("Phone number contains invalid characters.");
        RuleFor(x => x.PreferredTime).MaximumLength(100).When(x => x.PreferredTime is not null);
        RuleFor(x => x.Reason).MaximumLength(1000).When(x => x.Reason is not null);
        this.ApplySpamRules();
    }
}
