namespace ContactEmailApi.Application.Models.Email;

/// <summary>A binary attachment to include on an outgoing email.</summary>
public sealed record EmailAttachment(string FileName, string ContentType, byte[] Content);
