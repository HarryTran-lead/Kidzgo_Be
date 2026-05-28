using FluentValidation;

namespace Kidzgo.Application.ReportsV3.GenerateReport;

public sealed class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.PeriodId).NotEmpty();
        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.IdempotencyKey));
    }
}
