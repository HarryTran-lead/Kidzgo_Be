using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.GetReportPeriodById;

public sealed class GetReportPeriodByIdQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetReportPeriodByIdQuery, ReportPeriodDto>
{
    public async Task<Result<ReportPeriodDto>> Handle(
        GetReportPeriodByIdQuery query,
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
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (period is null)
        {
            return Result.Failure<ReportPeriodDto>(
                Error.NotFound("Report.PeriodNotFound", "Report period was not found."));
        }

        return Result.Success(ReportPeriodMapper.ToDto(period));
    }
}
