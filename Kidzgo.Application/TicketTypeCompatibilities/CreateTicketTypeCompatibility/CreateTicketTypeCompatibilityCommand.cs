using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

namespace Kidzgo.Application.TicketTypeCompatibilities.CreateTicketTypeCompatibility;

public sealed class CreateTicketTypeCompatibilityCommand : ICommand<TicketTypeCompatibilityDto>
{
    public Guid LearningTicketTypeId { get; init; }
    public Guid SlotTypeId { get; init; }
    public bool IsCompatible { get; init; }
}

