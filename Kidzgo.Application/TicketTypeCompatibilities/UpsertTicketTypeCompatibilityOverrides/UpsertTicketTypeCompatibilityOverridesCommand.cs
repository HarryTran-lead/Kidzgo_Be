using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpsertTicketTypeCompatibilityOverrides;

public sealed class UpsertTicketTypeCompatibilityOverridesCommand
    : ICommand<UpsertTicketTypeCompatibilityOverridesResponse>
{
    public Guid LearningTicketTypeId { get; init; }
    public List<UpsertTicketTypeCompatibilityOverrideItem> Items { get; init; } = new();
}

public sealed class UpsertTicketTypeCompatibilityOverrideItem
{
    public Guid SlotTypeId { get; init; }
    public bool? IsCompatible { get; init; }
}
