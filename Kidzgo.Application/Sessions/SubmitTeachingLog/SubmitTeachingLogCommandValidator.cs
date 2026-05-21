using FluentValidation;

namespace Kidzgo.Application.Sessions.SubmitTeachingLog;

public sealed class SubmitTeachingLogCommandValidator : AbstractValidator<SubmitTeachingLogCommand>
{
    public SubmitTeachingLogCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.ProgressStatus)
            .NotEmpty();
    }
}
