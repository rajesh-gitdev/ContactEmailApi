using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Models.Email;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Infrastructure.Email;

/// <summary>
/// Renders the built-in HTML templates that are shipped as embedded resources. Token
/// values are HTML-encoded before substitution to prevent HTML/script injection from
/// user-supplied content.
/// </summary>
public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly Assembly ResourceAssembly = typeof(EmailTemplateRenderer).Assembly;
    private static readonly ConcurrentDictionary<EmailTemplateType, string> Cache = new();

    private static readonly IReadOnlyDictionary<EmailTemplateType, string> FileNames =
        new Dictionary<EmailTemplateType, string>
        {
            [EmailTemplateType.Contact] = "contact.html",
            [EmailTemplateType.BusinessInquiry] = "business.html",
            [EmailTemplateType.Support] = "support.html",
            [EmailTemplateType.Career] = "career.html",
            [EmailTemplateType.Newsletter] = "welcome.html",
            [EmailTemplateType.Feedback] = "feedback.html",
            [EmailTemplateType.Callback] = "callback.html",
            [EmailTemplateType.Otp] = "otp.html",
            [EmailTemplateType.PasswordReset] = "passwordreset.html",
            [EmailTemplateType.Welcome] = "welcome.html",
            [EmailTemplateType.InternalNotification] = "internal.html"
        };

    public EmailRenderResult Render(
        EmailTemplateType template,
        string subject,
        IReadOnlyDictionary<string, string> tokens)
    {
        var body = Cache.GetOrAdd(template, LoadTemplate);
        var rendered = Substitute(body, tokens);
        return new EmailRenderResult(subject, rendered);
    }

    private static string Substitute(string template, IReadOnlyDictionary<string, string> tokens)
    {
        // Case-insensitive token map so callers can use {{Name}} or {{name}}.
        var map = new Dictionary<string, string>(tokens, StringComparer.OrdinalIgnoreCase);
        var result = new StringBuilder(template.Length + 128);
        var i = 0;

        while (i < template.Length)
        {
            var open = template.IndexOf("{{", i, StringComparison.Ordinal);
            if (open < 0)
            {
                result.Append(template, i, template.Length - i);
                break;
            }

            result.Append(template, i, open - i);
            var close = template.IndexOf("}}", open + 2, StringComparison.Ordinal);
            if (close < 0)
            {
                result.Append(template, open, template.Length - open);
                break;
            }

            var key = template.Substring(open + 2, close - (open + 2)).Trim();
            // Unknown tokens collapse to empty so stray placeholders never reach recipients.
            var value = map.TryGetValue(key, out var v) ? v : string.Empty;
            result.Append(WebUtility.HtmlEncode(value));
            i = close + 2;
        }

        return result.ToString();
    }

    private static string LoadTemplate(EmailTemplateType template)
    {
        var fileName = FileNames[template];
        var resourceName = Array.Find(
            ResourceAssembly.GetManifestResourceNames(),
            n => n.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException(
                $"Embedded email template '{fileName}' was not found. Ensure it is included as an EmbeddedResource.");
        }

        using var stream = ResourceAssembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
