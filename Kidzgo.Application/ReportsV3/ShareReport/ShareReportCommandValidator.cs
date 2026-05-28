using FluentValidation;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class ShareReportCommandValidator : AbstractValidator<ShareReportCommand>
{
    public ShareReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientContact).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProviderMessageId)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ProviderMessageId));
    }
}
