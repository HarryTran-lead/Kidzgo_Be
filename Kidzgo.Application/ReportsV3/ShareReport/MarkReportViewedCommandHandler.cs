using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class MarkReportViewedCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<MarkReportViewedCommand, ReportShareLogDto>
{
    public async Task<Result<ReportShareLogDto>> Handle(
        MarkReportViewedCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<ReportShareLogDto>(
                Error.NotFound("Report.NotFound", "Report was not found."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<ReportShareLogDto>(userResult.Error);
        }

        var currentUser = userResult.Value;
        if (currentUser.Role == UserRole.Parent)
        {
            var studentIds = await accessGuard.GetParentStudentIdsAsync(currentUser.Id, cancellationToken);
            if (!studentIds.Contains(report.StudentId))
            {
                return Result.Failure<ReportShareLogDto>(
                    Error.Unauthorized("Report.AccessDenied", "Parent can only mark viewed for linked students."));
            }
        }
        else if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff))
        {
            return Result.Failure<ReportShareLogDto>(
                Error.Unauthorized("Report.AccessDenied", "Current role cannot mark report viewed."));
        }

        var shareLog = await context.ReportShareLogs
            .Where(log =>
                log.StudentReportId == report.Id &&
                log.Channel == ReportShareChannel.App)
            .OrderByDescending(log => log.SentAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (shareLog is null)
        {
            return Result.Failure<ReportShareLogDto>(
                Error.NotFound("Report.ShareLogNotFound", "No app share log found for this report."));
        }

        shareLog.Status = ReportShareStatus.Viewed;
        shareLog.ViewedAt ??= VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(ReportDtoMapper.ToShareDto(shareLog));
    }
}
