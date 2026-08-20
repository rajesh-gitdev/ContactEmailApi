namespace ContactEmailApi.Application.Models.Common;

/// <summary>Ambient request metadata captured by the controller and passed to services.</summary>
public sealed record SubmissionContext(string? IpAddress, string? UserAgent);
