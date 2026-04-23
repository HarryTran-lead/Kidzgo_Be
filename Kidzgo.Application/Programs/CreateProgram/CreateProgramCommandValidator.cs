using FluentValidation;

namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramCommandValidator : AbstractValidator<CreateProgramCommand>
{
    public CreateProgramCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Program name is required")
            .MaximumLength(255).WithMessage("Program name must not exceed 255 characters");

        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Program code is required")
            .MaximumLength(10).WithMessage("Program code must not exceed 10 characters");

        RuleFor(command => command)
            .Must(command => !command.IsMakeup || !command.IsSupplementary)
            .WithMessage("A program cannot be both makeup and supplementary.");
    }
}
