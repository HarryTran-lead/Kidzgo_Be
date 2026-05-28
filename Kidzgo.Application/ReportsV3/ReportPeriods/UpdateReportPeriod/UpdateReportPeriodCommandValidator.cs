using FluentValidation;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.UpdateReportPeriod;

public sealed class UpdateReportPeriodCommandValidator : AbstractValidator<UpdateReportPeriodCommand>
{
    public UpdateReportPeriodCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Must(code => !string.IsNullOrWhiteSpace(code))
            .WithMessage("Code is required.")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate);
    }
}
