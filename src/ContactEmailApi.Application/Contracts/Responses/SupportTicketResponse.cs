namespace ContactEmailApi.Application.Contracts.Responses;

/// <summary>Returned after a support ticket is created.</summary>
public sealed record SupportTicketResponse(string TicketNumber, DateTimeOffset ReceivedAtUtc);
