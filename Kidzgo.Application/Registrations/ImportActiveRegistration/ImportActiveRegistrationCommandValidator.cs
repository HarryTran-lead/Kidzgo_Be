using FluentValidation;

namespace Kidzgo.Application.Registrations.ImportActiveRegistration;

public sealed class ImportActiveRegistrationCommandValidator : AbstractValidator<ImportActiveRegistrationCommand>
{
    public ImportActiveRegistrationCommandValidator()
    {
        RuleFor(command => command.StudentProfileId)
            .NotEmpty().WithMessage("Student profile ID is required.");

        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required.");

        RuleFor(command => command.TuitionPlanId)
            .NotEmpty().WithMessage("Tuition plan ID is required.");

        RuleFor(command => command.ActualStartDate)
            .NotEmpty().WithMessage("Actual start date is required.");

        RuleFor(command => command.UsedSessions)
            .GreaterThanOrEqualTo(0).WithMessage("Used sessions must be greater than or equal to 0.");

        RuleFor(command => command.RemainingSessions)
            .GreaterThan(0).WithMessage("Remaining sessions must be greater than 0 for an active imported registration.");
    }
}
