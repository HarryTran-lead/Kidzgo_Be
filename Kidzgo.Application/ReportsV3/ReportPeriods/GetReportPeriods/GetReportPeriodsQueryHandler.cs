using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.GetReportPeriods;

public sealed class GetReportPeriodsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetReportPeriodsQuery, PagedResult<ReportPeriodDto>>
{
    public async Task<Result<PagedResult<ReportPeriodDto>>> Handle(
        GetReportPeriodsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page <= 0 || query.PageSize <= 0)
        {
            return Result.Failure<PagedResult<ReportPeriodDto>>(
                Error.Validation("Report.InvalidPaging", "Page and pageSize must be greater than zero."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure<PagedResult<ReportPeriodDto>>(currentUserResult.Error);
        }

        if (!ReportPeriodAccessHelper.CanManage(currentUserResult.Value.Role))
        {
            return Result.Failure<PagedResult<ReportPeriodDto>>(
                Error.Unauthorized("Report.AccessDenied", "Only admin or management staff can manage report periods."));
        }

        var periodsQuery = context.ReportPeriods
            .AsNoTracking()
            .AsQueryable();

        if (query.Type.HasValue)
        {
            periodsQuery = periodsQuery.Where(x => x.Type == query.Type.Value);
        }

        if (query.From.HasValue)
        {
            periodsQuery = periodsQuery.Where(x => x.EndDate >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            periodsQuery = periodsQuery.Where(x => x.StartDate <= query.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var q = query.Q.Trim().ToLower();
            periodsQuery = periodsQuery.Where(
                x => x.Code.ToLower().Contains(q) ||
                     x.Name.ToLower().Contains(q));
        }

        periodsQuery = ApplySort(periodsQuery, query.SortBy, query.SortDir);

        var total = await periodsQuery.CountAsync(cancellationToken);
        var items = await periodsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<ReportPeriodDto>
        {
            Items = items.Select(ReportPeriodMapper.ToDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            HasNext = query.Page * query.PageSize < total
        });
    }

    private static IQueryable<ReportPeriod> ApplySort(
        IQueryable<ReportPeriod> query,
        string? sortBy,
        string? sortDir)
    {
        var descending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending
                ? query.OrderByDescending(x => x.Code).ThenByDescending(x => x.StartDate)
                : query.OrderBy(x => x.Code).ThenBy(x => x.StartDate),
            "name" => descending
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.StartDate)
                : query.OrderBy(x => x.Name).ThenBy(x => x.StartDate),
            "type" => descending
                ? query.OrderByDescending(x => x.Type).ThenByDescending(x => x.StartDate)
                : query.OrderBy(x => x.Type).ThenBy(x => x.StartDate),
            "enddate" => descending
                ? query.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate)
                : query.OrderBy(x => x.EndDate).ThenBy(x => x.StartDate),
            "createdat" => descending
                ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.StartDate)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.StartDate),
            _ => descending
                ? query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.StartDate).ThenBy(x => x.CreatedAt)
        };
    }
}
