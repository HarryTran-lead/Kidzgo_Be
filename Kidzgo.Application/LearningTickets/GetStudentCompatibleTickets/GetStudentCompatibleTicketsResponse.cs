namespace Kidzgo.Application.LearningTickets.GetStudentCompatibleTickets;

public sealed class GetStudentCompatibleTicketsResponse
{
    public bool Compatible { get; init; }
    public Guid? TicketItemId { get; init; }
    public Guid? TicketTypeId { get; init; }
    public string? TicketTypeCode { get; init; }
    public string Reason { get; init; } = null!;
}
