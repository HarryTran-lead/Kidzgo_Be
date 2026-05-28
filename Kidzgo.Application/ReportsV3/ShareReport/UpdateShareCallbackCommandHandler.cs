using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class UpdateShareCallbackCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateShareCallbackCommand, ReportShareLogDto>
{
    public async Task<Result<ReportShareLogDto>> Handle(
        UpdateShareCallbackCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<ReportShareLogDto>(
                Error.NotFound("Report.UserNotFound", "Current user was not found."));
        }

        if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff))
        {
            return Result.Failure<ReportShareLogDto>(
                Error.Unauthorized("Report.ShareCallbackDenied", "Only admin/management can process callbacks."));
        }

        var shareLog = await context.ReportShareLogs
            .FirstOrDefaultAsync(x => x.ProviderMessageId == command.ProviderMessageId, cancellationToken);

        if (shareLog is null)
        {
            return Result.Failure<ReportShareLogDto>(
                Error.NotFound("Report.ShareLogNotFound", "Share log was not found for provider message id."));
        }

        var targetStatus = command.Status;
        var viewedAt = command.ViewedAt;
        if (targetStatus == ReportShareStatus.Viewed && !viewedAt.HasValue)
        {
            viewedAt = VietnamTime.UtcNow();
        }

        shareLog.Status = targetStatus;
        shareLog.ViewedAt = viewedAt ?? shareLog.ViewedAt;
        shareLog.ErrorMessage = command.ErrorMessage;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(ReportDtoMapper.ToShareDto(shareLog));
    }
}
