using FluentValidation;

namespace Kidzgo.Application.Sessions.UpdateSessionsByClass;

public sealed class UpdateSessionsByClassCommandValidator : AbstractValidator<UpdateSessionsByClassCommand>
{
    public UpdateSessionsByClassCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty()
            .WithMessage("Class ID is required");

        RuleFor(command => command.PlannedDatetime)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("PlannedDatetime cannot be in the past")
            .When(command => command.PlannedDatetime.HasValue);

        RuleFor(command => command.SlotTypeId)
            .NotEqual(Guid.Empty)
            .WithMessage("SlotTypeId must not be empty")
            .When(command => command.SlotTypeId.HasValue);
    }
}

