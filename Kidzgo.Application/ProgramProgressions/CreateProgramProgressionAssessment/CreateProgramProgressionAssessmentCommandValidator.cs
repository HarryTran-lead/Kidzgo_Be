using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionAssessment;

public sealed class CreateProgramProgressionAssessmentCommandValidator : AbstractValidator<CreateProgramProgressionAssessmentCommand>
{
    public CreateProgramProgressionAssessmentCommandValidator()
    {
        RuleFor(x => x.SourceRegistrationId).NotEmpty();
    }
}
