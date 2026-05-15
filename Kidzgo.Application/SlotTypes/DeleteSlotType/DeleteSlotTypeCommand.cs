using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SlotTypes.DeleteSlotType;

public sealed class DeleteSlotTypeCommand : ICommand
{
    public Guid Id { get; init; }
}

