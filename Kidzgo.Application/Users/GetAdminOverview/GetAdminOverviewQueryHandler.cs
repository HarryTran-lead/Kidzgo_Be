using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetAdminOverview;

public sealed class GetAdminOverviewQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetAdminOverviewQuery, AdminOverviewResponse>
{
    public async Task<Result<AdminOverviewResponse>> Handle(
        GetAdminOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var fromDate = query.FromDate ?? now.AddMonths(-1);
        var toDate = query.ToDate ?? now.AddMonths(1);

        // Base queries with branch filter
        var branchesQuery = context.Branches.AsNoTracking();
        var classesQuery = context.Classes.AsNoTracking();
        var sessionsQuery = context.Sessions.AsNoTracking();
        var studentsQuery = context.Profiles
            .AsNoTracking()
            .Where(p => p.ProfileType == Domain.Users.ProfileType.Student && !p.IsDeleted && p.IsActive);
        var enrollmentsQuery = context.ClassEnrollments.AsNoTracking();
        var invoicesQuery = context.Invoices.AsNoTracking();
        var reportsQuery = context.StudentMonthlyReports.AsNoTracking();
        var ticketsQuery = context.Tickets.AsNoTracking();

        if (query.BranchId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.BranchId == query.BranchId.Value);
            sessionsQuery = sessionsQuery.Where(s => s.BranchId == query.BranchId.Value);
            studentsQuery = studentsQuery.Where(p => p.User.BranchId == query.BranchId.Value);
            invoicesQuery = invoicesQuery.Where(i => i.BranchId == query.BranchId.Value);
            ticketsQuery = ticketsQuery.Where(t => t.BranchId == query.BranchId.Value);
        }

        if (query.ClassId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.Id == query.ClassId.Value);
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == query.ClassId.Value);
            enrollmentsQuery = enrollmentsQuery.Where(ce => ce.ClassId == query.ClassId.Value);
            reportsQuery = reportsQuery.Where(r => r.ClassId == query.ClassId.Value);
            ticketsQuery = ticketsQuery.Where(t => t.ClassId == query.ClassId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            studentsQuery = studentsQuery.Where(p => p.Id == query.StudentProfileId.Value);
            enrollmentsQuery = enrollmentsQuery.Where(ce => ce.StudentProfileId == query.StudentProfileId.Value);
            invoicesQuery = invoicesQuery.Where(i => i.StudentProfileId == query.StudentProfileId.Value);
            reportsQuery = reportsQuery.Where(r => r.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.ProgramId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.ProgramId == query.ProgramId.Value);
        }

        // Statistics
        var statistics = new DashboardStatistics
        {
            TotalBranches = await branchesQuery.CountAsync(cancellationToken),
            TotalClasses = await classesQuery.CountAsync(cancellationToken),
            ActiveClasses = await classesQuery.CountAsync(c => c.Status == ClassStatus.Active, cancellationToken),
            TotalStudents = await studentsQuery.CountAsync(cancellationToken),
            TotalSessions = await sessionsQuery.CountAsync(cancellationToken),
            UpcomingSessions = await sessionsQuery.CountAsync(s => s.Status == SessionStatus.Scheduled && s.PlannedDatetime >= now, cancellationToken),
            TotalRevenue = await invoicesQuery
                .Where(i => i.Status == InvoiceStatus.Paid && i.IssuedAt >= fromDate && i.IssuedAt <= toDate)
                .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0,
            PendingPayments = await invoicesQuery
                .Where(i => i.Status == InvoiceStatus.Pending)
                .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0,
            PendingReports = await reportsQuery
                .CountAsync(r => r.Status == ReportStatus.Review || r.Status == ReportStatus.Draft, cancellationToken),
            OpenTickets = await ticketsQuery
                .CountAsync(t => t.Status != TicketStatus.Closed, cancellationToken)
        };

        // Branches
        var branches = await branchesQuery
            .Select(b => new BranchSummaryDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                ClassCount = b.Classes.Count(),
                StudentCount = b.Classes
                    .SelectMany(c => c.ClassEnrollments)
                    .Where(ce => ce.Status == EnrollmentStatus.Active)
                    .Select(ce => ce.StudentProfileId)
                    .Distinct()
                    .Count()
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
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                Status = c.Status.ToString(),
                EnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active),
                Capacity = c.Capacity
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

        // Students
        var students = await studentsQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new StudentSummaryDto
            {
                ProfileId = p.Id,
                DisplayName = p.DisplayName,
                BranchId = p.User.BranchId ?? Guid.Empty,
                BranchName = p.User.Branch != null ? p.User.Branch.Name : "",
                ActiveEnrollments = p.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active)
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

        // Pending Invoices
        var pendingInvoices = await invoicesQuery
            .Where(i => i.Status == InvoiceStatus.Pending)
            .OrderBy(i => i.DueDate)
            .Take(20)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.Id.ToString(), // TODO: Use actual invoice number if exists
                StudentName = i.StudentProfile.DisplayName,
                Amount = i.Amount,
                PaymentStatus = i.Status.ToString(),
                DueDate = i.DueDate.HasValue
                    ? VietnamTime.TreatAsVietnamLocal(i.DueDate.Value.ToDateTime(TimeOnly.MinValue))
                    : null
            })
            .ToListAsync(cancellationToken);

        // Pending Reports
        var pendingReports = await reportsQuery
            .Where(r => r.Status == ReportStatus.Review || r.Status == ReportStatus.Draft)
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

        return new AdminOverviewResponse
        {
            Statistics = statistics,
            Branches = branches,
            Classes = classes,
            UpcomingSessions = upcomingSessions,
            Students = students,
            RecentEnrollments = recentEnrollments,
            PendingInvoices = pendingInvoices,
            PendingReports = pendingReports,
            OpenTickets = openTickets
        };
    }
}

