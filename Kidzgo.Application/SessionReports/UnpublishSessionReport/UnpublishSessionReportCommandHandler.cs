using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.UnpublishSessionReport;

public sealed class UnpublishSessionReportCommandHandler(
    IDbContext context
) : ICommandHandler<UnpublishSessionReportCommand, UnpublishSessionReportResponse>
{
    public async Task<Result<UnpublishSessionReportResponse>> Handle(
        UnpublishSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.StudentProfile)
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<UnpublishSessionReportResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        if (sessionReport.Status != ReportStatus.Published)
        {
            return Result.Failure<UnpublishSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(
                    sessionReport.Status,
                    "unpublish"));
        }

        sessionReport.Status = ReportStatus.Approved;
        sessionReport.PublishedAt = null;
        sessionReport.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UnpublishSessionReportResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            Status = sessionReport.Status,
            PublishedAt = sessionReport.PublishedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}
