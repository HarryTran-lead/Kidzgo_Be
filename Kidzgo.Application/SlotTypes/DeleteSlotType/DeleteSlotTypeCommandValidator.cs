using FluentValidation;

namespace Kidzgo.Application.SlotTypes.DeleteSlotType;

public sealed class DeleteSlotTypeCommandValidator : AbstractValidator<DeleteSlotTypeCommand>
{
    public DeleteSlotTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

