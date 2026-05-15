using FluentValidation;

namespace Kidzgo.Application.TicketTypeCompatibilities.DeleteTicketTypeCompatibility;

public sealed class DeleteTicketTypeCompatibilityCommandValidator : AbstractValidator<DeleteTicketTypeCompatibilityCommand>
{
    public DeleteTicketTypeCompatibilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

