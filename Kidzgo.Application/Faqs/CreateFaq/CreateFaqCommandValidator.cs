using FluentValidation;

namespace Kidzgo.Application.Faqs.CreateFaq;

public sealed class CreateFaqCommandValidator : AbstractValidator<CreateFaqCommand>
{
    public CreateFaqCommandValidator()
    {
        RuleFor(command => command.CategoryId)
            .NotEmpty().WithMessage("Category id is required");

        RuleFor(command => command.Question)
            .NotEmpty().WithMessage("Question is required")
            .MaximumLength(500).WithMessage("Question must not exceed 500 characters");

        RuleFor(command => command.Answer)
            .NotEmpty().WithMessage("Answer is required")
            .MaximumLength(10000).WithMessage("Answer must not exceed 10000 characters");

        RuleFor(command => command.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0");
    }
}
