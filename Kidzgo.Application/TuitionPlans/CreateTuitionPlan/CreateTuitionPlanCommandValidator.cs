using FluentValidation;

namespace Kidzgo.Application.TuitionPlans.CreateTuitionPlan;

public sealed class CreateTuitionPlanCommandValidator : AbstractValidator<CreateTuitionPlanCommand>
{
    public CreateTuitionPlanCommandValidator()
    {
        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required");

        RuleFor(command => command.LevelId)
            .NotEmpty().WithMessage("Level ID is required");

        RuleFor(command => command.ModuleId)
            .NotEqual(Guid.Empty).WithMessage("Module ID must not be empty")
            .When(command => command.ModuleId.HasValue);

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(command => command.TotalSessions)
            .GreaterThan(0).WithMessage("Total sessions must be greater than 0");

        RuleFor(command => command.TuitionAmount)
            .GreaterThan(0).WithMessage("Tuition amount must be greater than 0");

        RuleFor(command => command.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters");

        RuleFor(command => command.LearningTicketTypeId)
            .NotEqual(Guid.Empty).WithMessage("Learning ticket type ID must not be empty")
            .When(command => command.LearningTicketTypeId.HasValue);
    }
}
