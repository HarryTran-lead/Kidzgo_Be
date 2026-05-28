using FluentValidation;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.DeleteReportTemplate;

public sealed class DeleteReportTemplateCommandValidator : AbstractValidator<DeleteReportTemplateCommand>
{
    public DeleteReportTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
