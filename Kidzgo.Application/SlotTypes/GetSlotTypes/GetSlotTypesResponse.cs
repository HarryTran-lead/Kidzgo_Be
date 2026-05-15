namespace Kidzgo.Application.SlotTypes.GetSlotTypes;

public sealed class GetSlotTypesResponse
{
    public List<SlotTypeDto> Items { get; init; } = new();
}

public sealed class SlotTypeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

