using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionAssessment;

public sealed class UpdateProgramProgressionAssessmentCommandValidator : AbstractValidator<UpdateProgramProgressionAssessmentCommand>
{
    public UpdateProgramProgressionAssessmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
