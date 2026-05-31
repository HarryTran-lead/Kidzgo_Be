using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.CreateReportPeriod;

public sealed class CreateReportPeriodCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<CreateReportPeriodCommand, ReportPeriodDto>
{
    public async Task<Result<ReportPeriodDto>> Handle(
        CreateReportPeriodCommand command,
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

        var trimmedCode = command.Code.Trim();
        var trimmedName = command.Name.Trim();

        var duplicatedCode = await context.ReportPeriods
            .AnyAsync(
                x => x.Code.ToLower() == trimmedCode.ToLower(),
                cancellationToken);

        if (duplicatedCode)
        {
            return Result.Failure<ReportPeriodDto>(
                Error.Conflict("Report.PeriodCodeExists", $"Report period code '{trimmedCode}' already exists."));
        }

        var period = new ReportPeriod
        {
            Id = Guid.NewGuid(),
            Code = trimmedCode,
            Name = trimmedName,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Type = command.Type,
            CreatedAt = VietnamTime.UtcNow()
        };

        context.ReportPeriods.Add(period);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(ReportPeriodMapper.ToDto(period));
    }
}
