namespace Kidzgo.Application.LearningTickets.GetStudentTicketBalance;

public sealed class GetStudentTicketBalanceResponse
{
    public Guid StudentProfileId { get; init; }
    public int Available { get; init; }
    public int Consumed { get; init; }
    public int TotalGranted { get; init; }
}
