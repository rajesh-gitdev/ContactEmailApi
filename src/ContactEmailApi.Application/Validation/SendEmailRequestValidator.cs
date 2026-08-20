using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class SendEmailRequestValidator : AbstractValidator<SendEmailRequest>
{
    public SendEmailRequestValidator()
    {
        RuleFor(x => x.To).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.ToDisplayName).MaximumLength(150).When(x => x.ToDisplayName is not null);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(100_000);
    }
}
