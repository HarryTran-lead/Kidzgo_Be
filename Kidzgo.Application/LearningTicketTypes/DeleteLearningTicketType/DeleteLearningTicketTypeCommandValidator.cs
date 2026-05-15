using FluentValidation;

namespace Kidzgo.Application.LearningTicketTypes.DeleteLearningTicketType;

public sealed class DeleteLearningTicketTypeCommandValidator : AbstractValidator<DeleteLearningTicketTypeCommand>
{
    public DeleteLearningTicketTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

