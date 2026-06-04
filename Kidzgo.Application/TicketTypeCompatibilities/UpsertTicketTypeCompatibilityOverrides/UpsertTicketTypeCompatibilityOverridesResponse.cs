using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpsertTicketTypeCompatibilityOverrides;

public sealed class UpsertTicketTypeCompatibilityOverridesResponse
{
    public Guid LearningTicketTypeId { get; init; }
    public int UpsertedCount { get; init; }
    public int RemovedCount { get; init; }
    public List<TicketTypeCompatibilityDto> Items { get; init; } = new();
}
