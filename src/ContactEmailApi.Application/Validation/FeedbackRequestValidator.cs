using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class FeedbackRequestValidator : AbstractValidator<FeedbackRequest>
{
    public FeedbackRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Message).NotEmpty().MinimumLength(5).MaximumLength(4000);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Name).MaximumLength(150).When(x => x.Name is not null);
        this.ApplySpamRules();
    }
}
