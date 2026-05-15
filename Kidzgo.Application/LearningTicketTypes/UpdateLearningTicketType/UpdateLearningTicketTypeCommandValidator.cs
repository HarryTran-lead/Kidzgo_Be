using FluentValidation;

namespace Kidzgo.Application.LearningTicketTypes.UpdateLearningTicketType;

public sealed class UpdateLearningTicketTypeCommandValidator : AbstractValidator<UpdateLearningTicketTypeCommand>
{
    public UpdateLearningTicketTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

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

