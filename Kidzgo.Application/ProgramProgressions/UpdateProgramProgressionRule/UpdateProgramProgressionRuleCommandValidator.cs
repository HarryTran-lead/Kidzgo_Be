using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionRule;

public sealed class UpdateProgramProgressionRuleCommandValidator : AbstractValidator<UpdateProgramProgressionRuleCommand>
{
    public UpdateProgramProgressionRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SourceLevelId)
            .NotEmpty()
            .WithMessage("SourceLevelId is required.");
    }
}
