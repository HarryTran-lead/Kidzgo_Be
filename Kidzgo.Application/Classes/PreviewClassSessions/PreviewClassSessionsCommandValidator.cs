using FluentValidation;

namespace Kidzgo.Application.Classes.PreviewClassSessions;

public sealed class PreviewClassSessionsCommandValidator : AbstractValidator<PreviewClassSessionsCommand>
{
    public PreviewClassSessionsCommandValidator()
    {
        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required");

        RuleFor(command => command.LevelId)
            .NotEmpty().WithMessage("Level ID is required");

        RuleFor(command => command.SyllabusId)
            .NotEmpty().WithMessage("Syllabus ID is required");

        RuleFor(command => command.StartModuleId)
            .NotEmpty().WithMessage("Start module ID is required");

        RuleFor(command => command.StartSessionIndex)
            .GreaterThan(0).WithMessage("Start session index must be greater than 0");

        RuleFor(command => command.SessionsToGenerate)
            .GreaterThan(0).WithMessage("Sessions to generate must be greater than 0");

        RuleFor(command => command.EndDate)
            .GreaterThanOrEqualTo(command => command.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .When(command => command.EndDate.HasValue);

        RuleFor(command => command.WeeklyScheduleSlots)
            .Must(slots => slots is { Count: > 0 })
            .WithMessage("Weekly schedule is required");
    }
}
