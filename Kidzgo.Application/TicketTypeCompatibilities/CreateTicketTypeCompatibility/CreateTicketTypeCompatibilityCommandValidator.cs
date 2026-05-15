using FluentValidation;

namespace Kidzgo.Application.TicketTypeCompatibilities.CreateTicketTypeCompatibility;

public sealed class CreateTicketTypeCompatibilityCommandValidator : AbstractValidator<CreateTicketTypeCompatibilityCommand>
{
    public CreateTicketTypeCompatibilityCommandValidator()
    {
        RuleFor(x => x.LearningTicketTypeId).NotEmpty();
        RuleFor(x => x.SlotTypeId).NotEmpty();
    }
}

