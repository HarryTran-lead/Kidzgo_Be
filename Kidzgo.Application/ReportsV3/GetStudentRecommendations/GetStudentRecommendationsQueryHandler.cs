using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetStudentRecommendations;

public sealed class GetStudentRecommendationsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentRecommendationsQuery, PagedResult<RecommendationDto>>
{
    public async Task<Result<PagedResult<RecommendationDto>>> Handle(
        GetStudentRecommendationsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page <= 0 || query.PageSize <= 0)
        {
            return Result.Failure<PagedResult<RecommendationDto>>(
                Error.Validation("Report.InvalidPaging", "Page and pageSize must be greater than zero."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<PagedResult<RecommendationDto>>(userResult.Error);
        }

        var currentUser = userResult.Value;
        if (currentUser.Role == UserRole.Parent)
        {
            return Result.Failure<PagedResult<RecommendationDto>>(
                Error.Unauthorized("Report.AccessDenied", "Parent cannot access internal recommendation list."));
        }

        var recommendationQuery = context.Recommendations
            .Where(r => r.StudentId == query.StudentId)
            .AsQueryable();

        if (currentUser.Role == UserRole.Teacher)
        {
            var teacherClassIds = await accessGuard.GetTeacherClassIdsAsync(currentUser.Id, cancellationToken);
            recommendationQuery = recommendationQuery.Where(r => r.ClassId.HasValue && teacherClassIds.Contains(r.ClassId.Value));
        }

        if (query.Status.HasValue)
        {
            recommendationQuery = recommendationQuery.Where(r => r.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            recommendationQuery = recommendationQuery.Where(r => r.Priority == query.Priority.Value);
        }

        if (query.DueFrom.HasValue)
        {
            recommendationQuery = recommendationQuery.Where(r => r.DueAt >= query.DueFrom.Value);
        }

        if (query.DueTo.HasValue)
        {
            recommendationQuery = recommendationQuery.Where(r => r.DueAt <= query.DueTo.Value);
        }

        if (query.Overdue.HasValue)
        {
            var now = VietnamTime.UtcNow();
            if (query.Overdue.Value)
            {
                recommendationQuery = recommendationQuery.Where(r =>
                    r.DueAt < now &&
                    r.Status != RecommendationStatus.Done &&
                    r.Status != RecommendationStatus.Rejected);
            }
            else
            {
                recommendationQuery = recommendationQuery.Where(r =>
                    r.DueAt >= now ||
                    r.Status == RecommendationStatus.Done ||
                    r.Status == RecommendationStatus.Rejected);
            }
        }

        recommendationQuery = ApplySort(recommendationQuery, query.SortBy, query.SortDir);

        var total = await recommendationQuery.CountAsync(cancellationToken);
        var items = await recommendationQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<RecommendationDto>
        {
            Items = items.Select(ReportDtoMapper.ToRecommendationDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            HasNext = query.Page * query.PageSize < total
        });
    }

    private static IQueryable<Recommendation> ApplySort(
        IQueryable<Recommendation> query,
        string? sortBy,
        string? sortDir)
    {
        var descending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "priority" => descending
                ? query.OrderByDescending(x => x.Priority).ThenBy(x => x.DueAt)
                : query.OrderBy(x => x.Priority).ThenBy(x => x.DueAt),
            "status" => descending
                ? query.OrderByDescending(x => x.Status).ThenBy(x => x.DueAt)
                : query.OrderBy(x => x.Status).ThenBy(x => x.DueAt),
            "dueat" => descending
                ? query.OrderByDescending(x => x.DueAt)
                : query.OrderBy(x => x.DueAt),
            _ => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };
    }
}
