using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.UnpublishMonthlyReport;

public sealed class UnpublishMonthlyReportCommandHandler(
    IDbContext context
) : ICommandHandler<UnpublishMonthlyReportCommand, UnpublishMonthlyReportResponse>
{
    public async Task<Result<UnpublishMonthlyReportResponse>> Handle(
        UnpublishMonthlyReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<UnpublishMonthlyReportResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        if (report.Status != ReportStatus.Published)
        {
            return Result.Failure<UnpublishMonthlyReportResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "unpublish"));
        }

        report.Status = ReportStatus.Approved;
        report.PublishedAt = null;
        report.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UnpublishMonthlyReportResponse
        {
            Id = report.Id,
            Status = report.Status.ToString(),
            PublishedAt = report.PublishedAt,
            UpdatedAt = report.UpdatedAt
        };
    }
}
