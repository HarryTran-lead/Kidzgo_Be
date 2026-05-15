namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

public sealed class GetTicketTypeCompatibilitiesResponse
{
    public List<TicketTypeCompatibilityDto> Items { get; init; } = new();
}

public sealed class TicketTypeCompatibilityDto
{
    public Guid Id { get; init; }
    public Guid LearningTicketTypeId { get; init; }
    public string LearningTicketTypeCode { get; init; } = null!;
    public Guid SlotTypeId { get; init; }
    public string SlotTypeCode { get; init; } = null!;
    public bool IsCompatible { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

