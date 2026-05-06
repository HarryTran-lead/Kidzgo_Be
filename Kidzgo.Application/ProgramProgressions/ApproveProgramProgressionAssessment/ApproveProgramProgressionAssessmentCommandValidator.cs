using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.ApproveProgramProgressionAssessment;

public sealed class ApproveProgramProgressionAssessmentCommandValidator : AbstractValidator<ApproveProgramProgressionAssessmentCommand>
{
    public ApproveProgramProgressionAssessmentCommandValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
    }
}
