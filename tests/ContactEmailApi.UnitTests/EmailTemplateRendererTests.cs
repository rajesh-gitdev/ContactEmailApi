using System.Collections.Generic;
using ContactEmailApi.Domain.Enums;
using ContactEmailApi.Infrastructure.Email;
using Xunit;

namespace ContactEmailApi.UnitTests;

public sealed class EmailTemplateRendererTests
{
    private readonly EmailTemplateRenderer _renderer = new();

    [Fact]
    public void Render_SubstitutesTokens()
    {
        var result = _renderer.Render(
            EmailTemplateType.Contact,
            "New message",
            new Dictionary<string, string>
            {
                ["Name"] = "Grace Hopper",
                ["Email"] = "grace@example.com",
                ["Subject"] = "Hi",
                ["Message"] = "Testing the renderer.",
                ["ReferenceCode"] = "CT-0001",
                ["AppName"] = "Contact API"
            });

        Assert.Equal("New message", result.Subject);
        Assert.Contains("Grace Hopper", result.HtmlBody);
        Assert.Contains("CT-0001", result.HtmlBody);
        // Unmatched placeholders must not leak into the output.
        Assert.DoesNotContain("{{", result.HtmlBody);
    }

    [Fact]
    public void Render_HtmlEncodesTokenValues_ToPreventInjection()
    {
        var result = _renderer.Render(
            EmailTemplateType.Contact,
            "s",
            new Dictionary<string, string>
            {
                ["Message"] = "<script>alert('xss')</script>"
            });

        Assert.DoesNotContain("<script>alert", result.HtmlBody);
        Assert.Contains("&lt;script&gt;", result.HtmlBody);
    }
}
