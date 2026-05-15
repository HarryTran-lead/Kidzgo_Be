using FluentValidation;

namespace Kidzgo.Application.LearningTicketTypes.CreateLearningTicketType;

public sealed class CreateLearningTicketTypeCommandValidator : AbstractValidator<CreateLearningTicketTypeCommand>
{
    public CreateLearningTicketTypeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

