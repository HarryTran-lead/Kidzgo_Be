using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetTeacherOverview;

public sealed class GetTeacherOverviewQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTeacherOverviewQuery, TeacherOverviewResponse>
{
    public async Task<Result<TeacherOverviewResponse>> Handle(
        GetTeacherOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var now = VietnamTime.UtcNow();
        var today = VietnamTime.ToVietnamDateOnly(now);
        var fromDate = query.FromDate ?? now.AddMonths(-1);
        var toDate = query.ToDate ?? now.AddMonths(1);

        // Get teacher's classes (main or assistant)
        var teacherClassesQuery = context.Classes
            .AsNoTracking()
            .Where(c => c.MainTeacherId == userId || c.AssistantTeacherId == userId);

        // Apply class filter
        if (query.ClassId.HasValue)
        {
            teacherClassesQuery = teacherClassesQuery.Where(c => c.Id == query.ClassId.Value);
        }

        // Get class IDs for filtering
        var teacherClassIds = await teacherClassesQuery.Select(c => c.Id).ToListAsync(cancellationToken);

        // Statistics (need to use filtered query)
        var statistics = new DashboardStatistics
        {
            TotalClasses = teacherClassIds.Count,
            TotalStudents = await context.ClassEnrollments
                .AsNoTracking()
                .Where(ce => teacherClassIds.Contains(ce.ClassId) && ce.Status == EnrollmentStatus.Active)
                .Select(ce => ce.StudentProfileId)
                .Distinct()
                .CountAsync(cancellationToken),
            UpcomingSessions = await context.Sessions
                .AsNoTracking()
                .Where(s => teacherClassIds.Contains(s.ClassId) &&
                            s.Status == SessionStatus.Scheduled &&
                            s.PlannedDatetime >= now &&
                            (!query.SessionId.HasValue || s.Id == query.SessionId.Value))
                .CountAsync(cancellationToken),
            PendingHomeworks = 0, // TODO: Implement homework count
            PendingReports = await context.StudentMonthlyReports
                .AsNoTracking()
                .Where(r => teacherClassIds.Contains(r.ClassId.Value) &&
                           (r.Status == ReportStatus.Draft || r.Status == ReportStatus.Review) &&
                           (!query.StudentProfileId.HasValue || r.StudentProfileId == query.StudentProfileId.Value))
                .CountAsync(cancellationToken),
            OpenTickets = await context.Tickets
                .AsNoTracking()
                .Where(t => teacherClassIds.Contains(t.ClassId.Value) &&
                           t.Status != TicketStatus.Closed)
                .CountAsync(cancellationToken)
        };

        // Classes
        var classes = await teacherClassesQuery
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClassSummaryDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                Role = c.MainTeacherId == userId ? "MainTeacher" : "AssistantTeacher",
                StudentCount = c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active),
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var classIds = teacherClassIds;

        // Apply student filter
        if (query.StudentProfileId.HasValue)
        {
            classIds = await context.ClassEnrollments
                .AsNoTracking()
                .Where(ce => classIds.Contains(ce.ClassId) && 
                           ce.StudentProfileId == query.StudentProfileId.Value &&
                           ce.Status == EnrollmentStatus.Active)
                .Select(ce => ce.ClassId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        // Upcoming Sessions
        var upcomingSessionsQuery = context.Sessions
            .AsNoTracking()
            .Where(s => classIds.Contains(s.ClassId) &&
                        s.Status == SessionStatus.Scheduled &&
                        s.PlannedDatetime >= now);

        if (query.SessionId.HasValue)
        {
            upcomingSessionsQuery = upcomingSessionsQuery.Where(s => s.Id == query.SessionId.Value);
        }

        var upcomingSessions = await upcomingSessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .Take(20)
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                PlannedDatetime = s.PlannedDatetime,
                Status = s.Status.ToString(),
                AttendanceMarked = s.Attendances.Any()
            })
            .ToListAsync(cancellationToken);

        // Students
        var studentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => classIds.Contains(ce.ClassId) && ce.Status == EnrollmentStatus.Active);

        if (query.StudentProfileId.HasValue)
        {
            studentsQuery = studentsQuery.Where(ce => ce.StudentProfileId == query.StudentProfileId.Value);
        }

        var students = await studentsQuery
            .Select(ce => new StudentSummaryDto
            {
                ProfileId = ce.StudentProfileId,
                DisplayName = ce.StudentProfile.DisplayName,
                ClassId = ce.ClassId,
                ClassCode = ce.Class.Code
            })
            .Distinct()
            .Take(50)
            .ToListAsync(cancellationToken);

        // Recent Attendances
        var recentAttendances = await context.Attendances
            .AsNoTracking()
            .Where(a => classIds.Contains(a.Session.ClassId) &&
                       a.Session.PlannedDatetime >= fromDate &&
                       a.Session.PlannedDatetime <= toDate)
            .GroupBy(a => new { a.SessionId, a.Session.Class.Code, a.Session.PlannedDatetime })
            .Select(g => new AttendanceSummaryDto
            {
                SessionId = g.Key.SessionId,
                ClassCode = g.Key.Code,
                SessionDate = g.Key.PlannedDatetime,
                PresentCount = g.Count(a => a.AttendanceStatus == Domain.Sessions.AttendanceStatus.Present),
                AbsentCount = g.Count(a => a.AttendanceStatus != Domain.Sessions.AttendanceStatus.Present)
            })
            .OrderByDescending(a => a.SessionDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Pending Homeworks (TODO: Implement when Homework entity is available)
        var pendingHomeworks = new List<HomeworkSummaryDto>();

        // Upcoming Exams
        var upcomingExams = await context.Exams
            .AsNoTracking()
            .Where(e => classIds.Contains(e.ClassId) &&
                       e.Date >= today)
            .OrderBy(e => e.Date)
            .Take(10)
            .Select(e => new ExamSummaryDto
            {
                Id = e.Id,
                Title = e.Description ?? e.ExamType.ToString(),
                ClassId = e.ClassId,
                ClassCode = e.Class.Code,
                ExamDate = VietnamTime.TreatAsVietnamLocal(e.Date.ToDateTime(TimeOnly.MinValue)),
                ExamType = e.ExamType.ToString()
            })
            .ToListAsync(cancellationToken);

        // Pending Reports
        var pendingReports = await context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => classIds.Contains(r.ClassId.Value) &&
                       (r.Status == ReportStatus.Draft || r.Status == ReportStatus.Review))
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .Select(r => new ReportSummaryDto
            {
                Id = r.Id,
                StudentName = r.StudentProfile.DisplayName,
                ClassCode = r.Class != null ? r.Class.Code : "",
                Status = r.Status.ToString(),
                ReportMonth = VietnamTime.TreatAsVietnamLocal(new DateTime(r.Year, r.Month, 1))
            })
            .ToListAsync(cancellationToken);

        // Open Tickets
        var openTickets = await context.Tickets
            .AsNoTracking()
            .Where(t => classIds.Contains(t.ClassId.Value) &&
                       t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new TicketSummaryDto
            {
                Id = t.Id,
                Title = t.Subject,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new TeacherOverviewResponse
        {
            Statistics = statistics,
            Classes = classes,
            UpcomingSessions = upcomingSessions,
            Students = students,
            RecentAttendances = recentAttendances,
            PendingHomeworks = pendingHomeworks,
            UpcomingExams = upcomingExams,
            PendingReports = pendingReports,
            OpenTickets = openTickets
        };
    }
}

