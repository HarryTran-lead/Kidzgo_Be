using FluentValidation;

namespace Kidzgo.Application.SlotTypes.CreateSlotType;

public sealed class CreateSlotTypeCommandValidator : AbstractValidator<CreateSlotTypeCommand>
{
    public CreateSlotTypeCommandValidator()
    {
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

