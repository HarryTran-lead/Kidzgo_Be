using FluentValidation;

namespace Kidzgo.Application.Faqs.UpdateFaq;

public sealed class UpdateFaqCommandValidator : AbstractValidator<UpdateFaqCommand>
{
    public UpdateFaqCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("FAQ id is required");

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
