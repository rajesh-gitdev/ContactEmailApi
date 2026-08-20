using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Validation;
using Xunit;

namespace ContactEmailApi.UnitTests;

public sealed class NewsletterSubscriptionValidatorTests
{
    private readonly NewsletterSubscriptionRequestValidator _validator = new();

    [Fact]
    public void Fails_WithoutConsent()
    {
        var result = _validator.Validate(new NewsletterSubscriptionRequest
        {
            Email = "reader@example.com",
            Consent = false
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(NewsletterSubscriptionRequest.Consent));
    }

    [Fact]
    public void Passes_WithConsentAndEmail()
    {
        var result = _validator.Validate(new NewsletterSubscriptionRequest
        {
            Email = "reader@example.com",
            Consent = true
        });

        Assert.True(result.IsValid);
    }
}
