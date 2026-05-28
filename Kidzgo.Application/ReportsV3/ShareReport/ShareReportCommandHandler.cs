using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class ShareReportCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<ShareReportCommand, ReportShareLogDto>
{
    public async Task<Result<ReportShareLogDto>> Handle(
        ShareReportCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentReports
            .Include(r => r.Class)
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
        if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff or UserRole.Teacher))
        {
            return Result.Failure<ReportShareLogDto>(
                Error.Unauthorized("Report.ShareDenied", "Current role cannot share reports."));
        }

        if (currentUser.Role == UserRole.Teacher &&
            report.Class.MainTeacherId != currentUser.Id &&
            report.Class.AssistantTeacherId != currentUser.Id)
        {
            return Result.Failure<ReportShareLogDto>(
                Error.Unauthorized("Report.ShareDenied", "Teacher can only share reports in their classes."));
        }

        if (!string.IsNullOrWhiteSpace(command.ProviderMessageId))
        {
            var existingLog = await context.ReportShareLogs
                .FirstOrDefaultAsync(
                    x => x.ProviderMessageId == command.ProviderMessageId,
                    cancellationToken);

            if (existingLog is not null)
            {
                return Result.Success(ReportDtoMapper.ToShareDto(existingLog));
            }
        }

        var now = VietnamTime.UtcNow();
        var shareLog = new ReportShareLog
        {
            Id = Guid.NewGuid(),
            StudentReportId = report.Id,
            RecipientName = command.RecipientName.Trim(),
            RecipientContact = command.RecipientContact.Trim(),
            Channel = command.Channel,
            Status = ReportShareStatus.Sent,
            ProviderMessageId = command.ProviderMessageId,
            SentAt = now
        };

        context.ReportShareLogs.Add(shareLog);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(ReportDtoMapper.ToShareDto(shareLog));
    }
}
