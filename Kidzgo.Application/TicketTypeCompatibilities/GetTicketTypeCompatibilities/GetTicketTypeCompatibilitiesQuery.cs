using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

public sealed class GetTicketTypeCompatibilitiesQuery : IQuery<GetTicketTypeCompatibilitiesResponse>
{
    public Guid? LearningTicketTypeId { get; init; }
    public Guid? SlotTypeId { get; init; }
}

