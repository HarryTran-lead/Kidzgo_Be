using FluentValidation;

namespace Kidzgo.Application.FaqCategories.UpdateFaqCategory;

public sealed class UpdateFaqCategoryCommandValidator : AbstractValidator<UpdateFaqCategoryCommand>
{
    public UpdateFaqCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Category id is required");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");

        RuleFor(command => command.Icon)
            .MaximumLength(100).WithMessage("Icon must not exceed 100 characters")
            .When(command => !string.IsNullOrWhiteSpace(command.Icon));

        RuleFor(command => command.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0");
    }
}
