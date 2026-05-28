using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetClassRiskAlerts;

public sealed class GetClassRiskAlertsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetClassRiskAlertsQuery, PagedResult<RiskAlertDto>>
{
    public async Task<Result<PagedResult<RiskAlertDto>>> Handle(
        GetClassRiskAlertsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page <= 0 || query.PageSize <= 0)
        {
            return Result.Failure<PagedResult<RiskAlertDto>>(
                Error.Validation("Report.InvalidPaging", "Page and pageSize must be greater than zero."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<PagedResult<RiskAlertDto>>(userResult.Error);
        }

        var currentUser = userResult.Value;
        if (currentUser.Role == UserRole.Parent)
        {
            return Result.Failure<PagedResult<RiskAlertDto>>(
                Error.Unauthorized("Report.AccessDenied", "Parent cannot access class risk alerts."));
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            var teacherClassIds = await accessGuard.GetTeacherClassIdsAsync(currentUser.Id, cancellationToken);
            if (!teacherClassIds.Contains(query.ClassId))
            {
                return Result.Failure<PagedResult<RiskAlertDto>>(
                    Error.Unauthorized("Report.AccessDenied", "Teacher can only view risk alerts in their classes."));
            }
        }

        var riskQuery = context.RiskAlerts
            .Where(r => r.ClassId == query.ClassId);

        if (query.RiskType.HasValue)
        {
            riskQuery = riskQuery.Where(r => r.RiskType == query.RiskType.Value);
        }

        if (query.Severity.HasValue)
        {
            riskQuery = riskQuery.Where(r => r.Severity == query.Severity.Value);
        }

        if (query.Status.HasValue)
        {
            riskQuery = riskQuery.Where(r => r.Status == query.Status.Value);
        }

        riskQuery = ApplySort(riskQuery, query.SortBy, query.SortDir);

        var total = await riskQuery.CountAsync(cancellationToken);
        var items = await riskQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<RiskAlertDto>
        {
            Items = items.Select(ReportDtoMapper.ToRiskDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            HasNext = query.Page * query.PageSize < total
        });
    }

    private static IQueryable<RiskAlert> ApplySort(
        IQueryable<RiskAlert> query,
        string? sortBy,
        string? sortDir)
    {
        var descending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "severity" => descending
                ? query.OrderByDescending(x => x.Severity).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Severity).ThenBy(x => x.CreatedAt),
            "status" => descending
                ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Status).ThenBy(x => x.CreatedAt),
            _ => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };
    }
}
