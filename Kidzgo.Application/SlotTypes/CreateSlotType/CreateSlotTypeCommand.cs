using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SlotTypes.GetSlotTypes;

namespace Kidzgo.Application.SlotTypes.CreateSlotType;

public sealed class CreateSlotTypeCommand : ICommand<SlotTypeDto>
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

