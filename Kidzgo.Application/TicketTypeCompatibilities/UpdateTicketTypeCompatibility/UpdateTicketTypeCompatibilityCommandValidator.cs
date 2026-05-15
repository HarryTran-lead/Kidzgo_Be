using FluentValidation;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpdateTicketTypeCompatibility;

public sealed class UpdateTicketTypeCompatibilityCommandValidator : AbstractValidator<UpdateTicketTypeCompatibilityCommand>
{
    public UpdateTicketTypeCompatibilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.LearningTicketTypeId).NotEmpty();
        RuleFor(x => x.SlotTypeId).NotEmpty();
    }
}

