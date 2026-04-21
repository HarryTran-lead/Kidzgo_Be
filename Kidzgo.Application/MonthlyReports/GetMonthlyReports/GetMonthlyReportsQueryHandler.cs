using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using ProfileType = Kidzgo.Domain.Users.ProfileType;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReports;

/// <summary>
/// Get list of Monthly Reports with authorization and filters
/// </summary>
public sealed class GetMonthlyReportsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMonthlyReportsQuery, GetMonthlyReportsResponse>
{
    public async Task<Result<GetMonthlyReportsResponse>> Handle(
        GetMonthlyReportsQuery query,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetMonthlyReportsResponse>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        // Build base query
        var reportsQuery = context.StudentMonthlyReports
            .Include(r => r.StudentProfile)
            .Include(r => r.Class)
                .ThenInclude(c => c!.Program)
            .AsQueryable();

        // Apply authorization filters based on user role
        if (currentUser.Role == UserRole.Teacher)
        {
            // Teacher can only view reports of their classes
            var teacherClassIds = await context.Classes
                .Where(c => c.MainTeacherId == currentUser.Id || c.AssistantTeacherId == currentUser.Id)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            if (!teacherClassIds.Any())
            {
                // Teacher has no classes, return empty page
                var emptyPage = new Page<MonthlyReportSummaryDto>(
                    new List<MonthlyReportSummaryDto>(),
                    0,
                    query.PageNumber,
                    query.PageSize);
                return new GetMonthlyReportsResponse { Reports = emptyPage };
            }

            reportsQuery = reportsQuery.Where(r => r.ClassId.HasValue && teacherClassIds.Contains(r.ClassId.Value));
        }
        else if (currentUser.Role == UserRole.Parent)
        {
            // Parent must resolve against the parent profile used by ParentStudentLinks.
            var parentProfile = await context.Profiles
                .FirstOrDefaultAsync(
                    p => p.UserId == currentUser.Id &&
                         p.ProfileType == ProfileType.Parent &&
                         p.IsActive,
                    cancellationToken);

            if (parentProfile is null)
            {
                return Result.Failure<GetMonthlyReportsResponse>(
                    Error.NotFound("MonthlyReport.Unauthorized", "User profile not found"));
            }

            // Get student profile IDs that belong to this parent
            var studentProfileIds = await context.ParentStudentLinks
                .Where(psl => psl.ParentProfileId == parentProfile.Id)
                .Select(psl => psl.StudentProfileId)
                .ToListAsync(cancellationToken);

            if (!studentProfileIds.Any())
            {
                // Parent has no children, return empty page
                var emptyPage = new Page<MonthlyReportSummaryDto>(
                    new List<MonthlyReportSummaryDto>(),
                    0,
                    query.PageNumber,
                    query.PageSize);
                return new GetMonthlyReportsResponse { Reports = emptyPage };
            }

            // Parent can only see published reports of their children
            reportsQuery = reportsQuery
                .Where(r => studentProfileIds.Contains(r.StudentProfileId) && r.Status == ReportStatus.Published);
        }
        
        // Student-only restriction should only apply to student users, not parent users
        // who may also have a student profile under the same login.
        if (currentUser.Role == UserRole.Student)
        {
            var studentProfile = await context.Profiles
                .FirstOrDefaultAsync(
                    p => p.UserId == currentUser.Id &&
                         p.ProfileType == ProfileType.Student &&
                         p.IsActive,
                    cancellationToken);

            if (studentProfile != null)
            {
                reportsQuery = reportsQuery
                    .Where(r => r.StudentProfileId == studentProfile.Id && r.Status == ReportStatus.Published);
            }
        }
        // Staff/Admin can view all reports (no additional filter)

        // Apply query filters
        if (query.StudentProfileId.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.ClassId.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.ClassId == query.ClassId.Value);
        }

        if (query.JobId.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.JobId == query.JobId.Value);
        }

        if (query.Month.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.Month == query.Month.Value);
        }

        if (query.Year.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.Year == query.Year.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<ReportStatus>(query.Status, true, out var status))
            {
                reportsQuery = reportsQuery.Where(r => r.Status == status);
            }
        }

        // Get total count before pagination
        int totalCount = await reportsQuery.CountAsync(cancellationToken);

        // Apply pagination and execute query
        var reports = await reportsQuery
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ThenByDescending(r => r.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var reportDtos = reports.Select(r => new MonthlyReportSummaryDto
        {
            Id = r.Id,
            StudentProfileId = r.StudentProfileId,
            StudentName = r.StudentProfile.DisplayName,
            ClassId = r.ClassId,
            ClassName = r.Class?.Title,
            ProgramId = r.Class?.ProgramId,
            ProgramName = r.Class?.Program?.Name,
            JobId = r.JobId,
            Month = r.Month,
            Year = r.Year,
            Status = r.Status.ToString(),
            PublishedAt = r.PublishedAt,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        var page = new Page<MonthlyReportSummaryDto>(
            reportDtos,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetMonthlyReportsResponse
        {
            Reports = page
        };
    }
}

