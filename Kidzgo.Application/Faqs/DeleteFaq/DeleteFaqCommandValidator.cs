using FluentValidation;

namespace Kidzgo.Application.Faqs.DeleteFaq;

public sealed class DeleteFaqCommandValidator : AbstractValidator<DeleteFaqCommand>
{
    public DeleteFaqCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("FAQ id is required");
    }
}
