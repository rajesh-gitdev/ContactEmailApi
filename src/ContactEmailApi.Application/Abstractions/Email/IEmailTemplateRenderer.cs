using ContactEmailApi.Application.Models.Email;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Application.Abstractions.Email;

/// <summary>Renders a built-in HTML email template with the supplied token values.</summary>
public interface IEmailTemplateRenderer
{
    /// <param name="template">Which template to render.</param>
    /// <param name="subject">Resolved subject line for the email.</param>
    /// <param name="tokens">Case-insensitive {{token}} replacements applied to the template body.</param>
    EmailRenderResult Render(EmailTemplateType template, string subject, IReadOnlyDictionary<string, string> tokens);
}
