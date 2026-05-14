using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LearningTickets.GetStudentTicketLedger;

public sealed class GetStudentTicketLedgerQuery : IQuery<GetStudentTicketLedgerResponse>
{
    public Guid StudentProfileId { get; init; }
}
