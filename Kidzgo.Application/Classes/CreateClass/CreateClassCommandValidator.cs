using FluentValidation;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required");

        RuleFor(command => command.LevelId)
            .NotEmpty().WithMessage("Level ID is required");

        RuleFor(command => command.StartModuleId)
            .NotEmpty().WithMessage("Start module ID is required");

        RuleFor(command => command.StartSessionIndex)
            .GreaterThan(0).WithMessage("Start session index must be greater than 0");

        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Class code is required")
            .MaximumLength(50).WithMessage("Class code must not exceed 50 characters");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Class title is required")
            .MaximumLength(255).WithMessage("Class title must not exceed 255 characters");

        RuleFor(command => command.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(command => command.EndDate)
            .GreaterThanOrEqualTo(command => command.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .When(command => command.EndDate.HasValue);

        RuleFor(command => command.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");

        RuleFor(command => command.WeeklyScheduleSlots)
            .Must(slots => slots is { Count: > 0 })
            .WithMessage("Weekly schedule is required");

        RuleFor(command => command.SessionsToGenerate)
            .GreaterThan(0).WithMessage("Sessions to generate must be greater than 0")
            .When(command => command.SessionsToGenerate.HasValue);

        RuleFor(command => command.SlotTypeId)
            .NotEqual(Guid.Empty).WithMessage("Slot type ID must not be empty")
            .When(command => command.SlotTypeId.HasValue);
    }
}

