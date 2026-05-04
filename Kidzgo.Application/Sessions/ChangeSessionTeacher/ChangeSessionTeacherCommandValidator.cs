using FluentValidation;

namespace Kidzgo.Application.Sessions.ChangeSessionTeacher;

public sealed class ChangeSessionTeacherCommandValidator : AbstractValidator<ChangeSessionTeacherCommand>
{
    public ChangeSessionTeacherCommandValidator()
    {
        RuleFor(command => command.SessionIds)
            .NotEmpty()
            .WithMessage("At least one session ID is required");

        RuleForEach(command => command.SessionIds)
            .NotEmpty()
            .WithMessage("Session ID cannot be empty");

        RuleFor(command => command.TeacherId)
            .NotEmpty()
            .WithMessage("Teacher ID is required");

        RuleFor(command => command.Role)
            .IsInEnum()
            .WithMessage("Role must be MainTeacher or Assistant");
    }
}

