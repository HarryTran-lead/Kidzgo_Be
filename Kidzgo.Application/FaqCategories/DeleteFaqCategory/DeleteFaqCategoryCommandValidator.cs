using FluentValidation;

namespace Kidzgo.Application.FaqCategories.DeleteFaqCategory;

public sealed class DeleteFaqCategoryCommandValidator : AbstractValidator<DeleteFaqCategoryCommand>
{
    public DeleteFaqCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Category id is required");
    }
}
