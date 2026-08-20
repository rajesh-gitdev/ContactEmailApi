using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

public sealed class CareerApplicationRequestValidator : AbstractValidator<CareerApplicationRequest>
{
    public CareerApplicationRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.PhoneNumber)
              .NotEmpty()
              .Matches(@"^\d{10}$")
              .WithMessage("Phone number must contain exactly 10 digits.");
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CoverLetter).MaximumLength(8000).When(x => x.CoverLetter is not null);
        RuleFor(x => x.ResumeUrl)
            .Must(BeAValidAbsoluteUrl).When(x => !string.IsNullOrWhiteSpace(x.ResumeUrl))
            .WithMessage("Resume URL must be a valid absolute URL.");
        RuleFor(x => x.LinkedInUrl)
            .Must(BeAValidAbsoluteUrl).When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl))
            .WithMessage("LinkedIn URL must be a valid absolute URL.");
        this.ApplySpamRules();
    }

    private static bool BeAValidAbsoluteUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var parsed)
        && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
}
