using FluentValidation;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.DeleteReportPeriod;

public sealed class DeleteReportPeriodCommandValidator : AbstractValidator<DeleteReportPeriodCommand>
{
    public DeleteReportPeriodCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
