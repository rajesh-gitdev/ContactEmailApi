namespace ContactEmailApi.Application.Contracts.Responses;

/// <summary>Generic acknowledgement returned when a submission is accepted for processing.</summary>
public sealed record SubmissionAcceptedResponse(string ReferenceCode, DateTimeOffset ReceivedAtUtc);
