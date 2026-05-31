using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.UpdateReportPeriod;

public sealed class UpdateReportPeriodCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateReportPeriodCommand, ReportPeriodDto>
{
    public async Task<Result<ReportPeriodDto>> Handle(
        UpdateReportPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure<ReportPeriodDto>(currentUserResult.Error);
        }

        if (!ReportPeriodAccessHelper.CanManage(currentUserResult.Value.Role))
        {
            return Result.Failure<ReportPeriodDto>(
                Error.Unauthorized("Report.AccessDenied", "Only admin, management staff, or teacher can manage report periods."));
        }

        var period = await context.ReportPeriods
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (period is null)
        {
            return Result.Failure<ReportPeriodDto>(
                Error.NotFound("Report.PeriodNotFound", "Report period was not found."));
        }

        var trimmedCode = command.Code.Trim();
        var trimmedName = command.Name.Trim();

        var duplicatedCode = await context.ReportPeriods
            .AnyAsync(
                x => x.Id != command.Id && x.Code.ToLower() == trimmedCode.ToLower(),
                cancellationToken);

        if (duplicatedCode)
        {
            return Result.Failure<ReportPeriodDto>(
                Error.Conflict("Report.PeriodCodeExists", $"Report period code '{trimmedCode}' already exists."));
        }

        period.Code = trimmedCode;
        period.Name = trimmedName;
        period.StartDate = command.StartDate;
        period.EndDate = command.EndDate;
        period.Type = command.Type;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(ReportPeriodMapper.ToDto(period));
    }
}
