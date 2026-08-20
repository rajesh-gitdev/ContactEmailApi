namespace ContactEmailApi.Application.Models.Email;

/// <summary>The rendered output of an email template.</summary>
public sealed record EmailRenderResult(string Subject, string HtmlBody);
