using FluentValidation;

namespace Kidzgo.Application.Sessions.UpdateTeachingLog;

public sealed class UpdateTeachingLogCommandValidator : AbstractValidator<UpdateTeachingLogCommand>
{
    public UpdateTeachingLogCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.ProgressStatus)
            .NotEmpty();
    }
}
