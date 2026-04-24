using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using ProfileType = Kidzgo.Domain.Users.ProfileType;

namespace Kidzgo.Application.SessionReports.GetSessionReports;

public sealed class GetSessionReportsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetSessionReportsQuery, GetSessionReportsResponse>
{
    public async Task<Result<GetSessionReportsResponse>> Handle(
        GetSessionReportsQuery query,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetSessionReportsResponse>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        // Build base query
        var reportsQuery = context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
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
                var emptyPage = new Page<SessionReportListItemDto>(
                    new List<SessionReportListItemDto>(),
                    0,
                    query.PageNumber,
                    query.PageSize);
                return new GetSessionReportsResponse { SessionReports = emptyPage };
            }

            reportsQuery = reportsQuery.Where(sr => teacherClassIds.Contains(sr.Session.ClassId));
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
                return Result.Failure<GetSessionReportsResponse>(
                    Error.NotFound("SessionReport.Unauthorized", "User profile not found"));
            }

            // Get student profile IDs that belong to this parent
            var studentProfileIds = await context.ParentStudentLinks
                .Where(psl => psl.ParentProfileId == parentProfile.Id)
                .Select(psl => psl.StudentProfileId)
                .ToListAsync(cancellationToken);

            if (!studentProfileIds.Any())
            {
                // Parent has no children, return empty page
                var emptyPage = new Page<SessionReportListItemDto>(
                    new List<SessionReportListItemDto>(),
                    0,
                    query.PageNumber,
                    query.PageSize);
                return new GetSessionReportsResponse { SessionReports = emptyPage };
            }

            // Parent can only see published reports of their children
            reportsQuery = reportsQuery
                .Where(sr => studentProfileIds.Contains(sr.StudentProfileId) && sr.Status == ReportStatus.Published);
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

            // Student can only see their own published reports
            if (studentProfile != null)
            {
                reportsQuery = reportsQuery
                    .Where(sr => sr.StudentProfileId == studentProfile.Id && sr.Status == ReportStatus.Published);
            }
        }
        // Staff/Admin can view all reports (no additional filter)

        // Apply query filters
        if (query.SessionId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.SessionId == query.SessionId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.TeacherUserId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.TeacherUserId == query.TeacherUserId.Value);
        }

        if (query.ClassId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.Session.ClassId == query.ClassId.Value);
        }

        if (query.FromDate.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.ReportDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.ReportDate <= query.ToDate.Value);
        }

        if (query.Status.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.Status == query.Status.Value);
        }

        var totalCount = await reportsQuery.CountAsync(cancellationToken);

        var reportRows = await reportsQuery
            .OrderByDescending(sr => sr.ReportDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(sr => new SessionReportListItemDto
            {
                Id = sr.Id,
                SessionId = sr.SessionId,
                SessionDate = sr.Session.PlannedDatetime,
                ClassId = sr.Session.ClassId,
                ClassCode = sr.Session.Class.Code,
                ClassTitle = sr.Session.Class.Title,
                StudentProfileId = sr.StudentProfileId,
                StudentName = sr.StudentProfile.DisplayName,
                TeacherUserId = sr.TeacherUserId,
                TeacherName = sr.TeacherUser.Name,
                ReportDate = sr.ReportDate,
                Feedback = sr.Feedback,
                Status = sr.Status.ToString(),
                IsMonthlyCompiled = sr.IsMonthlyCompiled,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var items = reportRows
            .Select(sr => new SessionReportListItemDto
            {
                Id = sr.Id,
                SessionId = sr.SessionId,
                SessionDate = VietnamTime.ToVietnamDateTime(sr.SessionDate),
                ClassId = sr.ClassId,
                ClassCode = sr.ClassCode,
                ClassTitle = sr.ClassTitle,
                StudentProfileId = sr.StudentProfileId,
                StudentName = sr.StudentName,
                TeacherUserId = sr.TeacherUserId,
                TeacherName = sr.TeacherName,
                ReportDate = sr.ReportDate,
                Feedback = sr.Feedback,
                Status = sr.Status,
                IsMonthlyCompiled = sr.IsMonthlyCompiled,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt
            })
            .ToList();

        var page = new Page<SessionReportListItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetSessionReportsResponse
        {
            SessionReports = page
        };
    }
}
