using FluentValidation;

namespace Kidzgo.Application.Homework.CreateHomeworkAssignment;

public sealed class CreateHomeworkAssignmentCommandValidator : AbstractValidator<CreateHomeworkAssignmentCommand>
{
    public CreateHomeworkAssignmentCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty()
            .WithMessage("Class ID is required");

        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required");

        RuleFor(command => command.DueAt)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("Due date should not be in the past")
            .When(command => command.DueAt.HasValue);

        RuleFor(command => command)
            .Must(command => !command.StartDate.HasValue ||
                             !command.DueAt.HasValue ||
                             command.StartDate.Value <= command.DueAt.Value)
            .WithMessage("Start date must be before or equal to due date");
    }
}

