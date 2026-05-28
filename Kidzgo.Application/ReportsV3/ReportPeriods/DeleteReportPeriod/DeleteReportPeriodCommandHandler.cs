using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.DeleteReportPeriod;

public sealed class DeleteReportPeriodCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeleteReportPeriodCommand>
{
    public async Task<Result> Handle(
        DeleteReportPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure(currentUserResult.Error);
        }

        if (currentUserResult.Value.Role != UserRole.Admin)
        {
            return Result.Failure(
                Error.Unauthorized("Report.AccessDenied", "Only admin can delete report periods."));
        }

        var period = await context.ReportPeriods
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (period is null)
        {
            return Result.Failure(
                Error.NotFound("Report.PeriodNotFound", "Report period was not found."));
        }

        var hasReportRuns = await context.ReportRuns
            .AnyAsync(x => x.ReportPeriodId == command.Id, cancellationToken);

        var hasStudentReports = await context.StudentReports
            .AnyAsync(x => x.ReportPeriodId == command.Id, cancellationToken);

        var hasRiskAlerts = await context.RiskAlerts
            .AnyAsync(x => x.ReportPeriodId == command.Id, cancellationToken);

        if (hasReportRuns || hasStudentReports || hasRiskAlerts)
        {
            return Result.Failure(
                Error.Conflict(
                    "Report.PeriodInUse",
                    "Cannot delete report period because it has related report data."));
        }

        context.ReportPeriods.Remove(period);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
