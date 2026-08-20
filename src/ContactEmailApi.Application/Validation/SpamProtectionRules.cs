using ContactEmailApi.Application.Contracts.Requests;
using FluentValidation;

namespace ContactEmailApi.Application.Validation;

/// <summary>Shared FluentValidation rules for anti-spam fields.</summary>
public static class SpamProtectionRules
{
    /// <summary>
    /// Applies the cheap, dependency-free honeypot check: the hidden field must be empty.
    /// (Timestamp/duplicate/reCAPTCHA checks are added in Phase 3.)
    /// </summary>
    public static void ApplySpamRules<T>(this AbstractValidator<T> validator)
        where T : ISpamProtectedRequest
    {
        validator.RuleFor(x => x.Honeypot)
            .Empty()
            .WithMessage("Spam detected.");
    }
}
