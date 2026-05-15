using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpdateTicketTypeCompatibility;

public sealed class UpdateTicketTypeCompatibilityCommand : ICommand<TicketTypeCompatibilityDto>
{
    public Guid Id { get; init; }
    public Guid LearningTicketTypeId { get; init; }
    public Guid SlotTypeId { get; init; }
    public bool IsCompatible { get; init; }
}

