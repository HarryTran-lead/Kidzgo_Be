namespace Kidzgo.Application.LearningTickets.GetStudentTicketLedger;

public sealed class GetStudentTicketLedgerResponse
{
    public List<StudentTicketLedgerItemDto> Items { get; init; } = new();
}

public sealed class StudentTicketLedgerItemDto
{
    public Guid Id { get; init; }
    public string TransactionType { get; init; } = null!;
    public int Quantity { get; init; }
    public string Reason { get; init; } = null!;
    public Guid? SessionId { get; init; }
    public Guid? AttendanceId { get; init; }
    public DateTime CreatedAt { get; init; }
}
