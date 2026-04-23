using FluentValidation;

namespace Kidzgo.Application.Programs.AssignProgramToBranch;

public sealed class AssignProgramToBranchCommandValidator : AbstractValidator<AssignProgramToBranchCommand>
{
    public AssignProgramToBranchCommandValidator()
    {
        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required");

        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");
    }
}
