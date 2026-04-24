using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetManagementStaffOverview;

public sealed class GetManagementStaffOverviewQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetManagementStaffOverviewQuery, ManagementStaffOverviewResponse>
{
    public async Task<Result<ManagementStaffOverviewResponse>> Handle(
        GetManagementStaffOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var now = VietnamTime.UtcNow();
        var fromDate = query.FromDate ?? now.AddMonths(-1);
        var toDate = query.ToDate ?? now.AddMonths(1);

        // Get staff's branch
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.BranchId.HasValue)
        {
            return Result.Failure<ManagementStaffOverviewResponse>(
                Domain.Common.Error.NotFound("User", "not found"));
        }

        var branchId = user.BranchId.Value;

        // Base queries filtered by branch (Management Staff - Operations)
        var leadsQuery = context.Leads
            .AsNoTracking()
            .Where(l => l.BranchPreference == branchId);
        var enrollmentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => ce.Class.BranchId == branchId);
        var classesQuery = context.Classes
            .AsNoTracking()
            .Where(c => c.BranchId == branchId);
        var sessionsQuery = context.Sessions
            .AsNoTracking()
            .Where(s => s.BranchId == branchId);
        var makeupCreditsQuery = context.MakeupCredits
            .AsNoTracking()
            .Where(mc => mc.StudentProfile.User.BranchId == branchId);
        var leaveRequestsQuery = context.LeaveRequests
            .AsNoTracking()
            .Where(lr => lr.Class.BranchId == branchId);
        var reportsQuery = context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => r.ClassId.HasValue && r.Class.BranchId == branchId);
        var ticketsQuery = context.Tickets
            .AsNoTracking()
            .Where(t => t.BranchId == branchId);

        // Apply entity filters
        if (query.ClassId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.Id == query.ClassId.Value);
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == query.ClassId.Value);
            enrollmentsQuery = enrollmentsQuery.Where(ce => ce.ClassId == query.ClassId.Value);
            leaveRequestsQuery = leaveRequestsQuery.Where(lr => lr.ClassId == query.ClassId.Value);
            reportsQuery = reportsQuery.Where(r => r.ClassId == query.ClassId.Value);
            ticketsQuery = ticketsQuery.Where(t => t.ClassId == query.ClassId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(ce => ce.StudentProfileId == query.StudentProfileId.Value);
            makeupCreditsQuery = makeupCreditsQuery.Where(mc => mc.StudentProfileId == query.StudentProfileId.Value);
            leaveRequestsQuery = leaveRequestsQuery.Where(lr => lr.StudentProfileId == query.StudentProfileId.Value);
            reportsQuery = reportsQuery.Where(r => r.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.LeadId.HasValue)
        {
            leadsQuery = leadsQuery.Where(l => l.Id == query.LeadId.Value);
        }

        if (query.EnrollmentId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(ce => ce.Id == query.EnrollmentId.Value);
        }

        // Statistics
        var statistics = new DashboardStatistics
        {
            TotalLeads = await leadsQuery.CountAsync(cancellationToken),
            TotalEnrollments = await enrollmentsQuery.CountAsync(cancellationToken),
            TotalClasses = await classesQuery.CountAsync(cancellationToken),
            UpcomingSessions = await sessionsQuery
                .CountAsync(s => s.Status == SessionStatus.Scheduled && s.PlannedDatetime >= now, cancellationToken),
            PendingMakeupCredits = await makeupCreditsQuery
                .CountAsync(mc => mc.Status == MakeupCreditStatus.Available, cancellationToken),
            PendingLeaveRequests = await leaveRequestsQuery
                .CountAsync(lr => lr.Status == LeaveRequestStatus.Pending, cancellationToken),
            PendingReports = await reportsQuery
                .CountAsync(r => r.Status == ReportStatus.Draft || r.Status == ReportStatus.Review, cancellationToken),
            OpenTickets = await ticketsQuery
                .CountAsync(t => t.Status != TicketStatus.Closed, cancellationToken)
        };

        // Recent Leads
        var recentLeads = await leadsQuery
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .Select(l => new LeadSummaryDto
            {
                Id = l.Id,
                Name = l.ContactName,
                PhoneNumber = l.Phone ?? "",
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Recent Enrollments
        var recentEnrollments = await enrollmentsQuery
            .Where(ce => ce.CreatedAt >= fromDate)
            .OrderByDescending(ce => ce.CreatedAt)
            .Take(20)
            .Select(ce => new EnrollmentSummaryDto
            {
                Id = ce.Id,
                ClassCode = ce.Class.Code,
                StudentName = ce.StudentProfile.DisplayName,
                EnrollDate = VietnamTime.TreatAsVietnamLocal(ce.EnrollDate.ToDateTime(TimeOnly.MinValue)),
                Status = ce.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Classes
        var classes = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .Take(20)
            .Select(c => new ClassSummaryDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                EnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active),
                Capacity = c.Capacity,
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Upcoming Sessions
        var upcomingSessionRows = await sessionsQuery
            .Where(s => s.Status == SessionStatus.Scheduled && s.PlannedDatetime >= now)
            .OrderBy(s => s.PlannedDatetime)
            .Take(20)
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
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
                PlannedDatetime = VietnamTime.ToVietnamDateTime(s.PlannedDatetime),
                Status = s.Status
            })
            .ToList();

        // Pending Makeup Credits
        var pendingMakeupCredits = await makeupCreditsQuery
            .Where(mc => mc.Status == MakeupCreditStatus.Available)
            .OrderBy(mc => mc.ExpiresAt)
            .Take(20)
            .Select(mc => new MakeupCreditSummaryDto
            {
                Id = mc.Id,
                StudentName = mc.StudentProfile.DisplayName,
                Status = mc.Status.ToString(),
                ExpiresAt = mc.ExpiresAt
            })
            .ToListAsync(cancellationToken);

        // Pending Leave Requests
        var pendingLeaveRequests = await leaveRequestsQuery
            .Where(lr => lr.Status == LeaveRequestStatus.Pending)
            .OrderBy(lr => lr.RequestedAt)
            .Take(20)
            .Select(lr => new LeaveRequestSummaryDto
            {
                Id = lr.Id,
                StudentName = lr.StudentProfile.DisplayName,
                RequestDate = lr.RequestedAt,
                Status = lr.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Pending Reports
        var pendingReports = await reportsQuery
            .Where(r => r.Status == ReportStatus.Draft || r.Status == ReportStatus.Review)
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
        var openTickets = await ticketsQuery
            .Where(t => t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new TicketSummaryDto
            {
                Id = t.Id,
                Title = t.Subject,
                Status = t.Status.ToString(),
                Priority = t.Category.ToString(),
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ManagementStaffOverviewResponse
        {
            Statistics = statistics,
            RecentLeads = recentLeads,
            RecentEnrollments = recentEnrollments,
            Classes = classes,
            UpcomingSessions = upcomingSessions,
            PendingMakeupCredits = pendingMakeupCredits,
            PendingLeaveRequests = pendingLeaveRequests,
            PendingReports = pendingReports,
            OpenTickets = openTickets
        };
    }
}

