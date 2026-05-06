using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionRule;

public sealed class CreateProgramProgressionRuleCommandValidator : AbstractValidator<CreateProgramProgressionRuleCommand>
{
    public CreateProgramProgressionRuleCommandValidator()
    {
        RuleFor(x => x.SourceProgramId).NotEmpty();
    }
}
