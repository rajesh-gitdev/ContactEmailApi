using ContactEmailApi.Application.Contracts.Requests;
using ContactEmailApi.Application.Validation;
using Xunit;

namespace ContactEmailApi.UnitTests;

public sealed class ContactRequestValidatorTests
{
    private readonly ContactRequestValidator _validator = new();

    private static ContactRequest Valid() => new()
    {
        Name = "Ada Lovelace",
        Email = "ada@example.com",
        Subject = "Hello",
        Message = "I would like to know more about your services please.",
        Honeypot = null
    };

    [Fact]
    public void Passes_ForWellFormedRequest()
    {
        var result = _validator.Validate(Valid());
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Fails_ForInvalidEmail(string email)
    {
        var result = _validator.Validate(Valid() with { Email = email });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ContactRequest.Email));
    }

    [Fact]
    public void Fails_ForShortMessage()
    {
        var result = _validator.Validate(Valid() with { Message = "hi" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ContactRequest.Message));
    }

    [Fact]
    public void Fails_WhenHoneypotFilled()
    {
        var result = _validator.Validate(Valid() with { Honeypot = "i-am-a-bot" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ContactRequest.Honeypot));
    }
}
