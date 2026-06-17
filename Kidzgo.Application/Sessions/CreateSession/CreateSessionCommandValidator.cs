using FluentValidation;

namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(c => c.ClassId)
            .NotEmpty();

        RuleFor(c => c.PlannedDatetime)
            .NotEqual(default(DateTime))
            .WithMessage("PlannedDatetime is required")
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("PlannedDatetime cannot be in the past");

        RuleFor(c => c.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("DurationMinutes must be greater than 0");
    }
}


