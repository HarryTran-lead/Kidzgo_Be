using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SlotTypes.GetSlotTypes;

namespace Kidzgo.Application.SlotTypes.UpdateSlotType;

public sealed class UpdateSlotTypeCommand : ICommand<SlotTypeDto>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

