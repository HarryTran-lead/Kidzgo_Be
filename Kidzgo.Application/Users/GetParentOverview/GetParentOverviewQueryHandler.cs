using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetParentOverview;

public sealed class GetParentOverviewQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentOverviewQuery, ParentOverviewResponse>
{
    public async Task<Result<ParentOverviewResponse>> Handle(
        GetParentOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var now = VietnamTime.UtcNow();
        var fromDate = query.FromDate ?? now.AddMonths(-1);
        var toDate = query.ToDate ?? now.AddMonths(1);
        var fromDateInVietnam = VietnamTime.ToVietnamDateOnly(fromDate);

        // Get parent profile
        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                     p.ProfileType == Domain.Users.ProfileType.Parent &&
                                     !p.IsDeleted && p.IsActive, cancellationToken);

        if (parentProfile == null)
        {
            return Result.Failure<ParentOverviewResponse>(
                Domain.Common.Error.NotFound("ParentProfile", "not found"));
        }

        // Get StudentId from context (token) - only get data for selected student
        var selectedStudentId = query.StudentProfileId ?? userContext.StudentId;

        if (!selectedStudentId.HasValue)
        {
            return Result.Failure<ParentOverviewResponse>(
                Domain.Common.Error.NotFound("StudentId", "No student selected in token"));
        }

        // Verify the student is linked to this parent
        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(link => link.ParentProfileId == parentProfile.Id && 
                             link.StudentProfileId == selectedStudentId.Value, 
                      cancellationToken);

        if (!isLinked)
        {
            return Result.Failure<ParentOverviewResponse>(
                Domain.Common.Error.NotFound("Student", "Student not linked to this parent"));
        }

        // Use only the selected student from token
        var studentProfileIds = new List<Guid> { selectedStudentId.Value };

        // Statistics
        var statistics = new DashboardStatistics
        {
            TotalStudents = studentProfileIds.Count,
            TotalClasses = await context.ClassEnrollments
                .AsNoTracking()
                .Where(ce => studentProfileIds.Contains(ce.StudentProfileId) && 
                           ce.Status == EnrollmentStatus.Active)
                .Select(ce => ce.ClassId)
                .Distinct()
                .CountAsync(cancellationToken),
            UpcomingSessions = await context.Sessions
                .AsNoTracking()
                .Where(s => s.Class.ClassEnrollments.Any(ce => 
                    studentProfileIds.Contains(ce.StudentProfileId) &&
                    ce.Status == EnrollmentStatus.Active) &&
                    s.Status == SessionStatus.Scheduled &&
                    s.PlannedDatetime >= now)
                .CountAsync(cancellationToken),
            AvailableMakeupCredits = await context.MakeupCredits
                .AsNoTracking()
                .CountAsync(mc => studentProfileIds.Contains(mc.StudentProfileId) &&
                                 mc.Status == MakeupCreditStatus.Available, cancellationToken),
            PendingHomeworks = await context.HomeworkStudents
                .AsNoTracking()
                .CountAsync(hs => hs.StudentProfileId == selectedStudentId.Value &&
                                  (hs.Status == Domain.Homework.HomeworkStatus.Assigned ||
                                   hs.Status == Domain.Homework.HomeworkStatus.Missing ||
                                   hs.Status == Domain.Homework.HomeworkStatus.Late), cancellationToken),
            PendingInvoices = await context.Invoices
                .AsNoTracking()
                .CountAsync(i => studentProfileIds.Contains(i.StudentProfileId) &&
                                i.Status == InvoiceStatus.Pending, cancellationToken),
            ActiveMissions = await context.MissionProgresses
                .AsNoTracking()
                .CountAsync(mp => studentProfileIds.Contains(mp.StudentProfileId) &&
                                 (mp.Status == MissionProgressStatus.Assigned || 
                                  mp.Status == MissionProgressStatus.InProgress), cancellationToken),
            TotalStars = await context.StarTransactions
                .AsNoTracking()
                .Where(st => studentProfileIds.Contains(st.StudentProfileId))
                .SumAsync(st => (int?)st.Amount, cancellationToken) ?? 0
        };

        // Student Profiles
        var studentProfilesRaw = await context.Profiles
            .AsNoTracking()
            .Where(p => studentProfileIds.Contains(p.Id))
            .Select(p => new
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                LevelText = p.StudentLevel != null ? p.StudentLevel.CurrentLevel : null,
                TotalStars = p.StarTransactions.Sum(st => st.Amount),
                Xp = p.StudentLevel != null ? p.StudentLevel.CurrentXp : 0
            })
            .ToListAsync(cancellationToken);

        var studentProfiles = studentProfilesRaw
            .Select(p => new StudentProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                Level = TryParseLevel(p.LevelText),
                TotalStars = p.TotalStars,
                Xp = p.Xp
            })
            .ToList();

        // Classes
        var classes = await context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => studentProfileIds.Contains(ce.StudentProfileId) &&
                        ce.Status == EnrollmentStatus.Active)
            .Select(ce => new ClassSummaryDto
            {
                Id = ce.ClassId,
                Code = ce.Class.Code,
                Title = ce.Class.Title,
                StudentProfileId = ce.StudentProfileId,
                Status = ce.Class.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var classIds = classes.Select(c => c.Id).Distinct().ToList();

        // Apply class filter
        if (query.ClassId.HasValue && classIds.Contains(query.ClassId.Value))
        {
            classIds = new List<Guid> { query.ClassId.Value };
        }

        var currentRegistrationQuery = context.Registrations
            .AsNoTracking()
            .Where(r => r.StudentProfileId == selectedStudentId.Value &&
                        (r.Status == RegistrationStatus.Studying ||
                         r.Status == RegistrationStatus.ClassAssigned ||
                         r.Status == RegistrationStatus.Paused));

        if (query.ClassId.HasValue && classIds.Contains(query.ClassId.Value))
        {
            currentRegistrationQuery = currentRegistrationQuery
                .Where(r => r.ClassId == query.ClassId.Value ||
                            r.SecondaryClassId == query.ClassId.Value);
        }

        var currentRegistration = await currentRegistrationQuery
            .OrderByDescending(r => r.Status == RegistrationStatus.Studying)
            .ThenByDescending(r => r.ActualStartDate ?? r.ExpectedStartDate ?? r.RegistrationDate)
            .Select(r => new
            {
                ProgramName = r.Program.Name,
                PackageName = r.TuitionPlan.Name,
                r.TotalSessions,
                r.UsedSessions,
                r.RemainingSessions
            })
            .FirstOrDefaultAsync(cancellationToken);

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

        var upcomingSessionRows = await upcomingSessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .Take(20)
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                StudentProfileId = studentProfileIds.First(), // TODO: Map correctly
                PlannedDatetime = s.PlannedDatetime,
                Status = s.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var upcomingSessions = upcomingSessionRows
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.ClassCode,
                StudentProfileId = s.StudentProfileId,
                PlannedDatetime = VietnamTime.ToVietnamDateTime(s.PlannedDatetime),
                Status = s.Status
            })
            .ToList();

        // Recent Attendances
        var recentAttendanceRows = await context.Attendances
            .AsNoTracking()
            .Where(a => studentProfileIds.Contains(a.StudentProfileId) &&
                       a.Session.PlannedDatetime >= fromDate &&
                       a.Session.PlannedDatetime <= toDate)
            .OrderByDescending(a => a.Session.PlannedDatetime)
            .Take(20)
            .Select(a => new AttendanceSummaryDto
            {
                SessionId = a.SessionId,
                ClassCode = a.Session.Class.Code,
                SessionDate = a.Session.PlannedDatetime,
                AttendanceStatus = a.AttendanceStatus.ToString()
            })
            .ToListAsync(cancellationToken);

        var recentAttendances = recentAttendanceRows
            .Select(a => new AttendanceSummaryDto
            {
                SessionId = a.SessionId,
                ClassCode = a.ClassCode,
                SessionDate = VietnamTime.ToVietnamDateTime(a.SessionDate),
                AttendanceStatus = a.AttendanceStatus
            })
            .ToList();

        // Makeup Credits
        var makeupCredits = await context.MakeupCredits
            .AsNoTracking()
            .Where(mc => studentProfileIds.Contains(mc.StudentProfileId))
            .OrderByDescending(mc => mc.CreatedAt)
            .Take(20)
            .Select(mc => new MakeupCreditSummaryDto
            {
                Id = mc.Id,
                Status = mc.Status.ToString(),
                ExpiresAt = mc.ExpiresAt,
                UsedSessionId = mc.UsedSessionId
            })
            .ToListAsync(cancellationToken);

        var pendingHomeworks = await context.HomeworkStudents
            .AsNoTracking()
            .Where(hs => hs.StudentProfileId == selectedStudentId.Value &&
                         (hs.Status == Domain.Homework.HomeworkStatus.Assigned ||
                          hs.Status == Domain.Homework.HomeworkStatus.Missing ||
                          hs.Status == Domain.Homework.HomeworkStatus.Late))
            .OrderBy(hs => hs.Assignment.DueAt)
            .Take(20)
            .Select(hs => new HomeworkSummaryDto
            {
                Id = hs.AssignmentId,
                Title = hs.Assignment.Title,
                ClassId = hs.Assignment.ClassId,
                ClassCode = hs.Assignment.Class.Code,
                StudentProfileId = hs.StudentProfileId,
                DueDate = hs.Assignment.DueAt,
                SubmissionStatus = hs.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Recent Exams
        var recentExams = await context.ExamResults
            .AsNoTracking()
            .Where(er => studentProfileIds.Contains(er.StudentProfileId) &&
                        er.Exam.Date >= fromDateInVietnam)
            .OrderByDescending(er => er.Exam.Date)
            .Take(20)
            .Select(er => new ExamSummaryDto
            {
                Id = er.ExamId,
                Title = er.Exam.Description ?? "Exam",
                ClassId = er.Exam.ClassId,
                ClassCode = er.Exam.Class.Code,
                StudentProfileId = er.StudentProfileId,
                ExamDate = VietnamTime.TreatAsVietnamLocal(er.Exam.Date.ToDateTime(TimeOnly.MinValue)),
                Score = er.Score
            })
            .ToListAsync(cancellationToken);

        // Reports
        var reports = await context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => studentProfileIds.Contains(r.StudentProfileId) &&
                       r.Status == ReportStatus.Published)
            .OrderByDescending(r => new DateTime(r.Year, r.Month, 1))
            .Take(20)
            .Select(r => new ReportSummaryDto
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                ClassCode = r.Class != null ? r.Class.Code : "",
                ReportMonth = VietnamTime.TreatAsVietnamLocal(new DateTime(r.Year, r.Month, 1)),
                Status = r.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Pending Invoices
        var pendingInvoices = await context.Invoices
            .AsNoTracking()
            .Where(i => studentProfileIds.Contains(i.StudentProfileId) &&
                       i.Status == InvoiceStatus.Pending)
            .OrderBy(i => i.DueDate)
            .Take(20)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.Id.ToString(),
                StudentProfileId = i.StudentProfileId,
                Amount = i.Amount,
                PaymentStatus = i.Status.ToString(),
                DueDate = i.DueDate.HasValue
                    ? VietnamTime.TreatAsVietnamLocal(i.DueDate.Value.ToDateTime(TimeOnly.MinValue))
                    : null
            })
            .ToListAsync(cancellationToken);

        var openInvoices = await context.Invoices
            .AsNoTracking()
            .Where(i => studentProfileIds.Contains(i.StudentProfileId) &&
                       (i.Status == InvoiceStatus.Pending ||
                        i.Status == InvoiceStatus.Overdue))
            .Select(i => new
            {
                i.Amount,
                i.DueDate,
                PaidAmount = i.Payments.Sum(p => (decimal?)p.Amount) ?? 0m
            })
            .ToListAsync(cancellationToken);

        var outstandingInvoices = openInvoices
            .Select(i => new
            {
                i.DueDate,
                RemainingAmount = Math.Max(0m, i.Amount - i.PaidAmount)
            })
            .Where(i => i.RemainingAmount > 0m)
            .ToList();

        var outstandingAmount = outstandingInvoices.Sum(i => i.RemainingAmount);
        DateOnly? nextDueDateOnly = outstandingInvoices
            .Where(i => i.DueDate.HasValue)
            .OrderBy(i => i.DueDate)
            .Select(i => i.DueDate)
            .FirstOrDefault();
        DateTime? nextDueDate = nextDueDateOnly.HasValue
            ? VietnamTime.TreatAsVietnamLocal(nextDueDateOnly.Value.ToDateTime(TimeOnly.MinValue))
            : null;
        var daysUntilDue = nextDueDateOnly.HasValue
            ? nextDueDateOnly.Value.DayNumber - VietnamTime.ToVietnamDateOnly(now).DayNumber
            : (int?)null;

        // Active Missions
        var activeMissions = await context.MissionProgresses
            .AsNoTracking()
            .Where(mp => studentProfileIds.Contains(mp.StudentProfileId) &&
                        (mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress))
            .OrderByDescending(mp => mp.Mission.CreatedAt)
            .Take(20)
            .Select(mp => new MissionSummaryDto
            {
                Id = mp.MissionId,
                Title = mp.Mission.Title,
                StudentProfileId = mp.StudentProfileId,
                Status = mp.Status.ToString(),
                StarReward = mp.Mission.RewardStars ?? 0
            })
            .ToListAsync(cancellationToken);

        // Open Tickets
        var openTickets = await context.Tickets
            .AsNoTracking()
            .Where(t => studentProfileIds.Contains(t.OpenedByProfileId.Value) &&
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

        var attendanceTotal = await context.Attendances
            .AsNoTracking()
            .CountAsync(a => a.StudentProfileId == selectedStudentId.Value &&
                             a.Session.PlannedDatetime >= fromDate &&
                             a.Session.PlannedDatetime <= toDate, cancellationToken);
        var attendancePresent = attendanceTotal == 0
            ? 0
            : await context.Attendances
                .AsNoTracking()
                .CountAsync(a => a.StudentProfileId == selectedStudentId.Value &&
                                 a.Session.PlannedDatetime >= fromDate &&
                                 a.Session.PlannedDatetime <= toDate &&
                                 a.AttendanceStatus == Domain.Sessions.AttendanceStatus.Present, cancellationToken);

        var homeworkTotal = await context.HomeworkStudents
            .AsNoTracking()
            .CountAsync(hs => hs.StudentProfileId == selectedStudentId.Value, cancellationToken);
        var completedHomework = homeworkTotal == 0
            ? 0
            : await context.HomeworkStudents
                .AsNoTracking()
                .CountAsync(hs => hs.StudentProfileId == selectedStudentId.Value &&
                                  (hs.Status == Domain.Homework.HomeworkStatus.Submitted ||
                                   hs.Status == Domain.Homework.HomeworkStatus.Graded), cancellationToken);

        var currentStreak = await context.AttendanceStreaks
            .AsNoTracking()
            .Where(s => s.StudentProfileId == selectedStudentId.Value)
            .OrderByDescending(s => s.AttendanceDate)
            .Select(s => (int?)s.CurrentStreak)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var unreadNotifications = await context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.Channel == Domain.Notifications.NotificationChannel.InApp &&
                             (n.RecipientUserId == userId || n.RecipientProfileId == selectedStudentId.Value) &&
                             n.ReadAt == null, cancellationToken);

        var selectedStudent = studentProfiles.FirstOrDefault();
        var selectedClass = classes.FirstOrDefault();

        return new ParentOverviewResponse
        {
            Statistics = statistics,
            StudentProfiles = studentProfiles,
            Classes = classes,
            UpcomingSessions = upcomingSessions,
            RecentAttendances = recentAttendances,
            MakeupCredits = makeupCredits,
            PendingHomeworks = pendingHomeworks,
            RecentExams = recentExams,
            Reports = reports,
            PendingInvoices = pendingInvoices,
            ActiveMissions = activeMissions,
            OpenTickets = openTickets,
            StudentInfo = selectedStudent,
            ClassInfo = selectedClass,
            AttendanceRate = attendanceTotal == 0
                ? 0
                : Math.Round((decimal)attendancePresent * 100 / attendanceTotal, 2),
            HomeworkCompletion = homeworkTotal == 0
                ? 0
                : Math.Round((decimal)completedHomework * 100 / homeworkTotal, 2),
            Xp = selectedStudent?.Xp ?? 0,
            Level = selectedStudent?.Level,
            Streak = currentStreak,
            Stars = selectedStudent?.TotalStars ?? 0,
            NextClasses = upcomingSessions,
            PendingApprovals = new List<ParentPendingApprovalDto>(),
            TuitionDue = outstandingAmount,
            ProgramName = currentRegistration?.ProgramName,
            PackageName = currentRegistration?.PackageName,
            TotalSessions = currentRegistration?.TotalSessions,
            UsedSessions = currentRegistration?.UsedSessions,
            RemainingSessions = currentRegistration?.RemainingSessions,
            OutstandingAmount = outstandingAmount,
            NextDueDate = nextDueDate,
            DaysUntilDue = daysUntilDue,
            UnreadNotifications = unreadNotifications
        };
}

    private static int? TryParseLevel(string? levelText)
    {
        if (string.IsNullOrWhiteSpace(levelText))
        {
            return null;
        }

        var digits = new string(levelText.Where(char.IsDigit).ToArray());

        return int.TryParse(digits, out var level)
            ? level
            : null;
    }
}

