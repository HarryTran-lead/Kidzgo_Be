using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionAssessment;

public sealed class CreateProgramProgressionAssessmentCommandValidator : AbstractValidator<CreateProgramProgressionAssessmentCommand>
{
    public CreateProgramProgressionAssessmentCommandValidator()
    {
        // At least one of SourceRegistrationId or ScheduleParticipantId must be provided
        RuleFor(x => x)
            .Must(x => x.SourceRegistrationId.HasValue || x.ScheduleParticipantId.HasValue)
            .WithMessage("Either SourceRegistrationId or ScheduleParticipantId must be provided.")
            .WithName("SourceRegistrationId or ScheduleParticipantId");
    }
}
