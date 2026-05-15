using FluentValidation;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Class ID is required");

        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required");

        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Class code is required")
            .MaximumLength(50).WithMessage("Class code must not exceed 50 characters");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Class title is required")
            .MaximumLength(255).WithMessage("Class title must not exceed 255 characters");

        RuleFor(command => command.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(command => command.EndDate)
            .NotNull().WithMessage("End date is required when weekly schedule is provided")
            .When(command => command.WeeklyScheduleSlots is { Count: > 0 })
            .GreaterThanOrEqualTo(command => command.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .When(command => command.EndDate.HasValue);

        RuleFor(command => command.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");

        RuleFor(command => command.SlotTypeId)
            .NotEqual(Guid.Empty).WithMessage("Slot type ID must not be empty")
            .When(command => command.SlotTypeId.HasValue);
    }
}

