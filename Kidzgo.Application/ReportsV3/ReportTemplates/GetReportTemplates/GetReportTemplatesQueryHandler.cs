using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.GetReportTemplates;

public sealed class GetReportTemplatesQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetReportTemplatesQuery, PagedResult<ReportTemplateDto>>
{
    public async Task<Result<PagedResult<ReportTemplateDto>>> Handle(
        GetReportTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page <= 0 || query.PageSize <= 0)
        {
            return Result.Failure<PagedResult<ReportTemplateDto>>(
                Error.Validation("Report.InvalidPaging", "Page and pageSize must be greater than zero."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure<PagedResult<ReportTemplateDto>>(currentUserResult.Error);
        }

        if (!ReportTemplateAccessHelper.CanView(currentUserResult.Value.Role))
        {
            return Result.Failure<PagedResult<ReportTemplateDto>>(
                Error.Unauthorized("Report.AccessDenied", "Only admin or management staff can view report templates."));
        }

        var templatesQuery = context.ReportTemplates
            .AsNoTracking()
            .AsQueryable();

        if (query.Type.HasValue)
        {
            templatesQuery = templatesQuery.Where(x => x.Type == query.Type.Value);
        }

        if (query.IsActive.HasValue)
        {
            templatesQuery = templatesQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var q = query.Q.Trim().ToLower();
            templatesQuery = templatesQuery.Where(
                x => x.Code.ToLower().Contains(q) || x.Name.ToLower().Contains(q));
        }

        templatesQuery = ApplySort(templatesQuery, query.SortBy, query.SortDir);

        var total = await templatesQuery.CountAsync(cancellationToken);
        var items = await templatesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<ReportTemplateDto>
        {
            Items = items.Select(ReportTemplateMapper.ToDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            HasNext = query.Page * query.PageSize < total
        });
    }

    private static IQueryable<ReportTemplate> ApplySort(
        IQueryable<ReportTemplate> query,
        string? sortBy,
        string? sortDir)
    {
        var descending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending
                ? query.OrderByDescending(x => x.Code).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Code).ThenBy(x => x.CreatedAt),
            "name" => descending
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Name).ThenBy(x => x.CreatedAt),
            "type" => descending
                ? query.OrderByDescending(x => x.Type).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Type).ThenBy(x => x.CreatedAt),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.CreatedAt),
            "createdat" => descending
                ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Code)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Code),
            _ => descending
                ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Code)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Code)
        };
    }
}
