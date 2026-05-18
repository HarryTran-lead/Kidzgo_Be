using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionRule;

public sealed class CreateProgramProgressionRuleCommandValidator : AbstractValidator<CreateProgramProgressionRuleCommand>
{
    public CreateProgramProgressionRuleCommandValidator()
    {
        RuleFor(x => x.SourceLevelId)
            .NotEmpty()
            .WithMessage("SourceLevelId is required.");
    }
}
