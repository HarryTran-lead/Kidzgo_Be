using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetStudentReports;

public sealed class GetStudentReportsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentReportsQuery, PagedResult<StudentReportListItemDto>>
{
    public async Task<Result<PagedResult<StudentReportListItemDto>>> Handle(
        GetStudentReportsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page <= 0 || query.PageSize <= 0)
        {
            return Result.Failure<PagedResult<StudentReportListItemDto>>(
                Error.Validation("Report.InvalidPaging", "Page and pageSize must be greater than zero."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<PagedResult<StudentReportListItemDto>>(userResult.Error);
        }

        var currentUser = userResult.Value;
        var reportsQuery = context.StudentReports
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Where(r => r.StudentId == query.StudentId)
            .AsQueryable();

        if (currentUser.Role == UserRole.Teacher)
        {
            var classIds = await accessGuard.GetTeacherClassIdsAsync(currentUser.Id, cancellationToken);
            reportsQuery = reportsQuery.Where(r => classIds.Contains(r.ClassId));
        }
        else if (currentUser.Role == UserRole.Parent)
        {
            var studentIds = await accessGuard.GetParentStudentIdsAsync(currentUser.Id, cancellationToken);
            if (!studentIds.Contains(query.StudentId))
            {
                return Result.Failure<PagedResult<StudentReportListItemDto>>(
                    Error.Unauthorized("Report.AccessDenied", "Parent can only access reports of their children."));
            }

            reportsQuery = reportsQuery.Where(r =>
                r.ReportType == StudentReportType.Parent &&
                r.IsParentPublished);
        }

        if (query.ClassId.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.ClassId == query.ClassId.Value);
        }

        if (query.BranchId.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.BranchId == query.BranchId.Value);
        }

        if (query.PeriodId.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.ReportPeriodId == query.PeriodId.Value);
        }

        if (query.ReportType.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.ReportType == query.ReportType.Value);
        }

        if (query.Status.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var q = query.Q.Trim().ToLowerInvariant();
            reportsQuery = reportsQuery.Where(r =>
                r.Student.DisplayName.ToLower().Contains(q) ||
                r.Class.Title.ToLower().Contains(q) ||
                (r.SummaryText != null && r.SummaryText.ToLower().Contains(q)));
        }

        if (query.From.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.CreatedAt <= query.To.Value);
        }

        reportsQuery = ApplySort(reportsQuery, query.SortBy, query.SortDir);

        var total = await reportsQuery.CountAsync(cancellationToken);
        var items = await reportsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var mappedItems = items.Select(ReportDtoMapper.ToListItem).ToList();

        return Result.Success(new PagedResult<StudentReportListItemDto>
        {
            Items = mappedItems,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            HasNext = query.Page * query.PageSize < total
        });
    }

    private static IQueryable<StudentReport> ApplySort(
        IQueryable<StudentReport> query,
        string? sortBy,
        string? sortDir)
    {
        var descending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "status" => descending
                ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.Status).ThenBy(x => x.CreatedAt),
            "reporttype" => descending
                ? query.OrderByDescending(x => x.ReportType).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.ReportType).ThenBy(x => x.CreatedAt),
            _ => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };
    }
}
