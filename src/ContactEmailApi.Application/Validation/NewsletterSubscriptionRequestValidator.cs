using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class NewsletterSubscriptionRequestValidator : AbstractValidator<NewsletterSubscriptionRequest>
{
    public NewsletterSubscriptionRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Name).MaximumLength(150).When(x => x.Name is not null);
        RuleFor(x => x.Consent).Equal(true).WithMessage("Consent is required to subscribe.");
        this.ApplySpamRules();
    }
}
