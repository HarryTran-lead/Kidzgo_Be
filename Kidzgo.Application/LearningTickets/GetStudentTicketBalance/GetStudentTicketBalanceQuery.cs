using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LearningTickets.GetStudentTicketBalance;

public sealed class GetStudentTicketBalanceQuery : IQuery<GetStudentTicketBalanceResponse>
{
    public Guid StudentProfileId { get; init; }
}
