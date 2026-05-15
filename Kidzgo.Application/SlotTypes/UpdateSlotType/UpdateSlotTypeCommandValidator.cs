using FluentValidation;

namespace Kidzgo.Application.SlotTypes.UpdateSlotType;

public sealed class UpdateSlotTypeCommandValidator : AbstractValidator<UpdateSlotTypeCommand>
{
    public UpdateSlotTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

