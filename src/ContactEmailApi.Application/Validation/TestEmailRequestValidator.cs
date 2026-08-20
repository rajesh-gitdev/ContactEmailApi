using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class TestEmailRequestValidator : AbstractValidator<TestEmailRequest>
{
    public TestEmailRequestValidator()
    {
        RuleFor(x => x.To).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.To));
    }
}
