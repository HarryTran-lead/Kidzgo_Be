using FluentValidation;

namespace Kidzgo.Application.ReportsV3.PublishReportToParent;

public sealed class PublishReportToParentCommandValidator : AbstractValidator<PublishReportToParentCommand>
{
    public PublishReportToParentCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
