using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Dashboard;

public sealed class GetDashboardQueryHandler(
    IDbContext context
) : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        var startDate = query.StartDate ?? VietnamTime.UtcNow().AddMonths(-1);
        var endDate = query.EndDate ?? VietnamTime.UtcNow();
        var branchId = query.BranchId;

        var response = new DashboardResponse
        {
            Attendance = await GetAttendanceStats(branchId, startDate, endDate, cancellationToken),
            Homework = await GetHomeworkStats(branchId, startDate, endDate, cancellationToken),
            Leave = await GetLeaveStats(branchId, startDate, endDate, cancellationToken),
            MakeupCredits = await GetMakeupCreditStats(branchId, cancellationToken),
            Students = await GetStudentStats(branchId, startDate, endDate, cancellationToken),
            Enrollments = await GetEnrollmentStats(branchId, startDate, endDate, cancellationToken),
            Leads = await GetLeadStats(branchId, startDate, endDate, cancellationToken),
            PlacementTests = await GetPlacementTestStats(branchId, startDate, endDate, cancellationToken),
            Finance = await GetFinanceStats(branchId, startDate, endDate, cancellationToken),
            HumanResources = await GetHrStats(branchId, startDate, endDate, cancellationToken)
        };

        return Result.Success(response);
    }

    private async Task<AttendanceStats> GetAttendanceStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Attendances
            .Include(s => s.Session)
            .Where(s => s.Session.PlannedDatetime >= startDate && s.Session.PlannedDatetime <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(s => s.Session.BranchId == branchId);
        }

        var attendances = await query.ToListAsync(cancellationToken);
        var total = attendances.Count;
        var present = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present);
        var absent = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent);
        var makeup = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Makeup);

        return new AttendanceStats
        {
            TotalSessions = total,
            PresentCount = present,
            AbsentCount = absent,
            LateCount = makeup,
            AttendanceRate = total > 0 ? Math.Round((double)present / total * 100, 2) : 0,

            TotalAttendanceRecords = total,
            UniqueSessionCount = attendances.Select(a => a.SessionId).Distinct().Count(),
            PresentRate = total > 0 ? Math.Round((double)present / total * 100, 2) : 0,
            AbsentRate = total > 0 ? Math.Round((double)absent / total * 100, 2) : 0,
            MakeupRate = total > 0 ? Math.Round((double)makeup / total * 100, 2) : 0,
            StatusBreakdown =
            [
                new StatusBreakdownItem
                {
                    Status = AttendanceStatus.Present.ToString(),
                    Count = present,
                    Percentage = total > 0 ? Math.Round((double)present / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = AttendanceStatus.Absent.ToString(),
                    Count = absent,
                    Percentage = total > 0 ? Math.Round((double)absent / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = AttendanceStatus.Makeup.ToString(),
                    Count = makeup,
                    Percentage = total > 0 ? Math.Round((double)makeup / total * 100, 2) : 0
                }
            ]
        };
    }

    private async Task<HomeworkStats> GetHomeworkStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .ThenInclude(a => a.Class)
            .Where(hs => hs.Assignment.DueAt.HasValue &&
                         hs.Assignment.DueAt.Value >= startDate &&
                         hs.Assignment.DueAt.Value <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(hs => hs.Assignment.Class.BranchId == branchId);
        }

        var homeworkStudents = await query.ToListAsync(cancellationToken);
        var now = VietnamTime.UtcNow();
        var total = homeworkStudents.Count;
        var assigned = homeworkStudents.Count(h => h.Status == HomeworkStatus.Assigned);
        var submitted = homeworkStudents.Count(h => h.Status == HomeworkStatus.Submitted);
        var graded = homeworkStudents.Count(h => h.Status == HomeworkStatus.Graded);
        var late = homeworkStudents.Count(h => h.Status == HomeworkStatus.Late);
        var missing = homeworkStudents.Count(h => h.Status == HomeworkStatus.Missing);
        var overdue = homeworkStudents.Count(h => (h.Status == HomeworkStatus.Assigned || h.Status == HomeworkStatus.Late) &&
                                                  h.Assignment.DueAt.HasValue &&
                                                  h.Assignment.DueAt.Value < now);

        return new HomeworkStats
        {
            Total = total,
            Pending = assigned,
            Submitted = submitted,
            Graded = graded,
            Overdue = overdue,

            TotalHomeworkSubmissions = total,
            AssignedCount = assigned,
            SubmittedCount = submitted,
            GradedCount = graded,
            LateCount = late,
            MissingCount = missing,
            OverdueCount = overdue,
            SubmissionRate = total > 0 ? Math.Round((double)(submitted + graded + late) / total * 100, 2) : 0,
            GradedRate = total > 0 ? Math.Round((double)graded / total * 100, 2) : 0,
            StatusBreakdown =
            [
                new StatusBreakdownItem
                {
                    Status = HomeworkStatus.Assigned.ToString(),
                    Count = assigned,
                    Percentage = total > 0 ? Math.Round((double)assigned / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = HomeworkStatus.Submitted.ToString(),
                    Count = submitted,
                    Percentage = total > 0 ? Math.Round((double)submitted / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = HomeworkStatus.Graded.ToString(),
                    Count = graded,
                    Percentage = total > 0 ? Math.Round((double)graded / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = HomeworkStatus.Late.ToString(),
                    Count = late,
                    Percentage = total > 0 ? Math.Round((double)late / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = HomeworkStatus.Missing.ToString(),
                    Count = missing,
                    Percentage = total > 0 ? Math.Round((double)missing / total * 100, 2) : 0
                }
            ]
        };
    }

    private async Task<LeaveStats> GetLeaveStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var startDateOnly = VietnamTime.ToVietnamDateOnly(startDate);
        var endDateOnly = VietnamTime.ToVietnamDateOnly(endDate);

        var query = context.LeaveRequests
            .Where(l => l.RequestedAt >= startDate && l.RequestedAt <= endDate)
            .Where(l => l.SessionDate >= startDateOnly && l.SessionDate <= endDateOnly);

        if (branchId.HasValue)
        {
            query = query.Where(l => l.Class.BranchId == branchId);
        }

        var leaves = await query.ToListAsync(cancellationToken);
        var total = leaves.Count;
        var pending = leaves.Count(l => l.Status == LeaveRequestStatus.Pending);
        var approved = leaves.Count(l => l.Status == LeaveRequestStatus.Approved);
        var rejected = leaves.Count(l => l.Status == LeaveRequestStatus.Rejected);

        return new LeaveStats
        {
            Total = total,
            Pending = pending,
            Approved = approved,
            Rejected = rejected,

            TotalRequests = total,
            PendingRequests = pending,
            ApprovedRequests = approved,
            RejectedRequests = rejected,
            ApprovalRate = total > 0 ? Math.Round((double)approved / total * 100, 2) : 0,
            StatusBreakdown =
            [
                new StatusBreakdownItem
                {
                    Status = LeaveRequestStatus.Pending.ToString(),
                    Count = pending,
                    Percentage = total > 0 ? Math.Round((double)pending / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = LeaveRequestStatus.Approved.ToString(),
                    Count = approved,
                    Percentage = total > 0 ? Math.Round((double)approved / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = LeaveRequestStatus.Rejected.ToString(),
                    Count = rejected,
                    Percentage = total > 0 ? Math.Round((double)rejected / total * 100, 2) : 0
                }
            ]
        };
    }

    private async Task<MakeupCreditStats> GetMakeupCreditStats(Guid? branchId, CancellationToken cancellationToken)
    {
        var query = context.MakeupCredits
            .Include(m => m.SourceSession)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(m => m.SourceSession.BranchId == branchId);
        }

        var credits = await query.ToListAsync(cancellationToken);
        var total = credits.Count;
        var used = credits.Count(c => c.Status == MakeupCreditStatus.Used);
        var transferred = credits.Count(c => c.Status == MakeupCreditStatus.Transferred);
        var available = credits.Count(c => c.Status == MakeupCreditStatus.Available);
        var expired = credits.Count(c => c.Status == MakeupCreditStatus.Expired);

        return new MakeupCreditStats
        {
            TotalCredits = total,
            UsedCredits = used,
            AvailableCredits = available,
            TransferredCredits = transferred,

            TotalCreditsIssued = total,
            UsedCreditsCount = used,
            AvailableCreditsCount = available,
            TransferredCreditsCount = transferred,
            ExpiredCreditsCount = expired,
            UtilizationRate = total > 0 ? Math.Round((double)(used + transferred) / total * 100, 2) : 0,
            StatusBreakdown =
            [
                new StatusBreakdownItem
                {
                    Status = MakeupCreditStatus.Available.ToString(),
                    Count = available,
                    Percentage = total > 0 ? Math.Round((double)available / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = MakeupCreditStatus.Used.ToString(),
                    Count = used,
                    Percentage = total > 0 ? Math.Round((double)used / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = MakeupCreditStatus.Transferred.ToString(),
                    Count = transferred,
                    Percentage = total > 0 ? Math.Round((double)transferred / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = MakeupCreditStatus.Expired.ToString(),
                    Count = expired,
                    Percentage = total > 0 ? Math.Round((double)expired / total * 100, 2) : 0
                }
            ]
        };
    }

    private async Task<StudentStats> GetStudentStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Profiles
            .Where(p => p.ProfileType == ProfileType.Student);

        if (branchId.HasValue)
        {
            query = query.Where(p => p.ClassEnrollments.Any(e => e.Class.BranchId == branchId));
        }

        var students = await query.ToListAsync(cancellationToken);
        var vietnamNow = VietnamTime.NowInVietnam();
        var thisMonth = VietnamTime.TreatAsVietnamLocal(new DateTime(vietnamNow.Year, vietnamNow.Month, 1));
        var total = students.Count;
        var active = students.Count(p => p.IsActive);
        var inactive = students.Count(p => !p.IsActive);
        var newThisMonth = students.Count(p => p.CreatedAt >= thisMonth);
        var newInSelectedRange = students.Count(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

        return new StudentStats
        {
            Total = total,
            Active = active,
            Inactive = inactive,
            NewThisMonth = newThisMonth,

            TotalStudents = total,
            ActiveStudents = active,
            InactiveStudents = inactive,
            NewStudentsThisMonth = newThisMonth,
            NewStudentsInSelectedRange = newInSelectedRange,
            ActiveStudentRate = total > 0 ? Math.Round((double)active / total * 100, 2) : 0
        };
    }

    private async Task<EnrollmentStats> GetEnrollmentStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.ClassEnrollments.AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(e => e.Class.BranchId == branchId);
        }

        var enrollments = await query.ToListAsync(cancellationToken);
        var vietnamNow = VietnamTime.NowInVietnam();
        var thisMonth = VietnamTime.TreatAsVietnamLocal(new DateTime(vietnamNow.Year, vietnamNow.Month, 1));
        var total = enrollments.Count;
        var active = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var paused = enrollments.Count(e => e.Status == EnrollmentStatus.Paused);
        var dropped = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped);
        var newThisMonth = enrollments.Count(e => e.CreatedAt >= thisMonth);
        var newInSelectedRange = enrollments.Count(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate);
        var activeRate = total > 0 ? Math.Round((double)active / total * 100, 2) : 0;

        return new EnrollmentStats
        {
            // Legacy
            Total = total,
            Active = active,
            Paused = paused,
            Dropped = dropped,
            NewThisMonth = newThisMonth,

            // Clear
            TotalEnrollments = total,
            ActiveEnrollments = active,
            PausedEnrollments = paused,
            DroppedEnrollments = dropped,
            NewEnrollmentsThisMonth = newThisMonth,
            NewEnrollmentsInSelectedRange = newInSelectedRange,
            ActiveEnrollmentRate = activeRate,
            StatusBreakdown =
            [
                new StatusBreakdownItem
                {
                    Status = EnrollmentStatus.Active.ToString(),
                    Count = active,
                    Percentage = total > 0 ? Math.Round((double)active / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = EnrollmentStatus.Paused.ToString(),
                    Count = paused,
                    Percentage = total > 0 ? Math.Round((double)paused / total * 100, 2) : 0
                },
                new StatusBreakdownItem
                {
                    Status = EnrollmentStatus.Dropped.ToString(),
                    Count = dropped,
                    Percentage = total > 0 ? Math.Round((double)dropped / total * 100, 2) : 0
                }
            ]
        };
    }

    private async Task<LeadStats> GetLeadStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Leads
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(l => l.BranchPreference == branchId);
        }

        var leads = await query.ToListAsync(cancellationToken);

        var total = leads.Count;
        var newLeads = leads.Count(l => l.Status == LeadStatus.New);
        var contacted = leads.Count(l => l.Status == LeadStatus.Contacted);
        var qualified = leads.Count(l => l.Status == LeadStatus.BookedTest || l.Status == LeadStatus.TestDone);
        var enrolled = leads.Count(l => l.Status == LeadStatus.Enrolled);
        var lost = leads.Count(l => l.Status == LeadStatus.Lost);
        var conversionRate = total > 0 ? Math.Round((double)enrolled / total * 100, 2) : 0;
        var qualificationRate = total > 0 ? Math.Round((double)qualified / total * 100, 2) : 0;

        return new LeadStats
        {
            Total = total,
            New = newLeads,
            Contacted = contacted,
            Qualified = qualified,
            Enrolled = enrolled,
            NoShow = 0, // LeadStatus doesn't have NoShow
            Lost = lost,
            ConversionRate = conversionRate,
            TotalTouchCount = leads.Sum(l => l.TouchCount),

            TotalLeads = total,
            NewLeads = newLeads,
            ContactedLeads = contacted,
            QualifiedLeads = qualified,
            EnrolledLeads = enrolled,
            LostLeads = lost,
            NoShowLeads = 0,
            QualificationRate = qualificationRate,
            StatusBreakdown =
            [
                new StatusBreakdownItem { Status = LeadStatus.New.ToString(), Count = newLeads, Percentage = total > 0 ? Math.Round((double)newLeads / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = LeadStatus.Contacted.ToString(), Count = contacted, Percentage = total > 0 ? Math.Round((double)contacted / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = LeadStatus.BookedTest.ToString(), Count = leads.Count(l => l.Status == LeadStatus.BookedTest), Percentage = total > 0 ? Math.Round((double)leads.Count(l => l.Status == LeadStatus.BookedTest) / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = LeadStatus.TestDone.ToString(), Count = leads.Count(l => l.Status == LeadStatus.TestDone), Percentage = total > 0 ? Math.Round((double)leads.Count(l => l.Status == LeadStatus.TestDone) / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = LeadStatus.Enrolled.ToString(), Count = enrolled, Percentage = total > 0 ? Math.Round((double)enrolled / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = LeadStatus.Lost.ToString(), Count = lost, Percentage = total > 0 ? Math.Round((double)lost / total * 100, 2) : 0 }
            ]
        };
    }

    private async Task<PlacementTestStats> GetPlacementTestStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.PlacementTests
            .Where(p => p.ScheduledAt.HasValue &&
                        p.ScheduledAt.Value >= startDate &&
                        p.ScheduledAt.Value <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(p =>
                (p.Class != null && p.Class.BranchId == branchId) ||
                (p.Lead != null && p.Lead.BranchPreference == branchId));
        }

        var tests = await query.ToListAsync(cancellationToken);
        var total = tests.Count;
        var scheduled = tests.Count(t => t.Status == PlacementTestStatus.Scheduled);
        var completed = tests.Count(t => t.Status == PlacementTestStatus.Completed);
        var noShow = tests.Count(t => t.Status == PlacementTestStatus.NoShow);
        var cancelled = tests.Count(t => t.Status == PlacementTestStatus.Cancelled);

        return new PlacementTestStats
        {
            Total = total,
            Scheduled = scheduled,
            Completed = completed,
            NoShow = noShow,
            Enrolled = completed, // Need to check if enrolled

            TotalTests = total,
            ScheduledTests = scheduled,
            CompletedTests = completed,
            NoShowTests = noShow,
            CancelledTests = cancelled,
            CompletionRate = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0,
            NoShowRate = total > 0 ? Math.Round((double)noShow / total * 100, 2) : 0,
            StatusBreakdown =
            [
                new StatusBreakdownItem { Status = PlacementTestStatus.Scheduled.ToString(), Count = scheduled, Percentage = total > 0 ? Math.Round((double)scheduled / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = PlacementTestStatus.Completed.ToString(), Count = completed, Percentage = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = PlacementTestStatus.NoShow.ToString(), Count = noShow, Percentage = total > 0 ? Math.Round((double)noShow / total * 100, 2) : 0 },
                new StatusBreakdownItem { Status = PlacementTestStatus.Cancelled.ToString(), Count = cancelled, Percentage = total > 0 ? Math.Round((double)cancelled / total * 100, 2) : 0 }
            ]
        };
    }

    private async Task<FinanceStats> GetFinanceStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Invoices
            .Include(i => i.Payments)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(i => i.BranchId == branchId);
        }

        var invoices = await query.ToListAsync(cancellationToken);

        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();
        var pendingInvoices = invoices.Where(i => i.Status == InvoiceStatus.Pending).ToList();
        var overdueInvoices = invoices.Where(i => i.Status == InvoiceStatus.Overdue).ToList();
        var cancelledInvoices = invoices.Where(i => i.Status == InvoiceStatus.Cancelled).ToList();
        var allPayments = invoices.SelectMany(i => i.Payments).ToList();
        var totalBilled = invoices.Sum(i => i.Amount);
        var totalCollected = allPayments.Sum(p => p.Amount);
        var collectedInRange = allPayments
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startDate && p.PaidAt.Value <= endDate)
            .Sum(p => p.Amount);
        var totalOutstanding = GetOutstanding(pendingInvoices) + GetOutstanding(overdueInvoices);
        var collectionRate = totalBilled > 0 ? Math.Round((double)(totalCollected / totalBilled) * 100, 2) : 0;
        var invoiceTotal = invoices.Count;

        decimal GetOutstanding(IEnumerable<Invoice> selectedInvoices)
        {
            return selectedInvoices.Sum(i => i.Amount - i.Payments.Sum(p => p.Amount));
        }

        return new FinanceStats
        {
            TotalRevenue = totalCollected,
            MonthRevenue = collectedInRange,
            PayOSRevenue = allPayments.Where(p => p.Method == PaymentMethod.Payos).Sum(p => p.Amount),
            CashRevenue = allPayments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount),
            OutstandingDebt = totalOutstanding,
            PaidInvoices = paidInvoices.Count,
            PendingInvoices = pendingInvoices.Count,
            OverdueInvoices = overdueInvoices.Count,

            TotalBilledAmount = totalBilled,
            TotalCollectedAmount = totalCollected,
            CollectedInSelectedRange = collectedInRange,
            TotalOutstandingAmount = totalOutstanding,
            CollectionRate = collectionRate,
            InvoiceStatusBreakdown =
            [
                new StatusBreakdownItem { Status = InvoiceStatus.Paid.ToString(), Count = paidInvoices.Count, Percentage = invoiceTotal > 0 ? Math.Round((double)paidInvoices.Count / invoiceTotal * 100, 2) : 0 },
                new StatusBreakdownItem { Status = InvoiceStatus.Pending.ToString(), Count = pendingInvoices.Count, Percentage = invoiceTotal > 0 ? Math.Round((double)pendingInvoices.Count / invoiceTotal * 100, 2) : 0 },
                new StatusBreakdownItem { Status = InvoiceStatus.Overdue.ToString(), Count = overdueInvoices.Count, Percentage = invoiceTotal > 0 ? Math.Round((double)overdueInvoices.Count / invoiceTotal * 100, 2) : 0 },
                new StatusBreakdownItem { Status = InvoiceStatus.Cancelled.ToString(), Count = cancelledInvoices.Count, Percentage = invoiceTotal > 0 ? Math.Round((double)cancelledInvoices.Count / invoiceTotal * 100, 2) : 0 }
            ]
        };
    }

    private async Task<HrStats> GetHrStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Get teachers and staff
        var staffQuery = context.Users.Where(u => u.Role != UserRole.Parent);
        
        if (branchId.HasValue)
        {
            staffQuery = staffQuery.Where(u => u.BranchId == branchId);
        }

        var staff = await staffQuery.ToListAsync(cancellationToken);

        // Get session roles for work hours
        var sessionRolesQuery = context.SessionRoles
            .Include(sr => sr.Session)
            .Where(sr => sr.Session.PlannedDatetime >= startDate && sr.Session.PlannedDatetime <= endDate);

        if (branchId.HasValue)
        {
            sessionRolesQuery = sessionRolesQuery.Where(sr => sr.Session.BranchId == branchId);
        }

        var sessionRoles = await sessionRolesQuery.ToListAsync(cancellationToken);

        // Get payroll runs and payroll payments
        var payrollRunsQuery = context.PayrollRuns.AsQueryable();
        if (branchId.HasValue)
        {
            payrollRunsQuery = payrollRunsQuery.Where(p => p.BranchId == branchId);
        }

        var payrollRuns = await payrollRunsQuery.ToListAsync(cancellationToken);

        var payrollPaymentsQuery = context.PayrollPayments
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startDate && p.PaidAt.Value <= endDate);
        if (branchId.HasValue)
        {
            payrollPaymentsQuery = payrollPaymentsQuery.Where(p => p.PayrollRun.BranchId == branchId);
        }

        var payrollPayments = await payrollPaymentsQuery.ToListAsync(cancellationToken);
        var payrollRunTotal = payrollRuns.Count;
        var processedRuns = payrollRuns.Count(p => p.Status == PayrollRunStatus.Approved || p.Status == PayrollRunStatus.Paid);
        var draftRuns = payrollRuns.Count(p => p.Status == PayrollRunStatus.Draft);
        var totalWorkHours = sessionRoles.Sum(sr => sr.Session.DurationMinutes) / 60d;
        var totalPayroll = payrollPayments.Sum(p => p.Amount);

        return new HrStats
        {
            TotalStaff = staff.Count,
            TotalWorkHours = totalWorkHours,
            TotalPayroll = totalPayroll,
            PayrollProcessed = processedRuns,
            PayrollPending = draftRuns,

            TeacherCount = staff.Count(u => u.Role == UserRole.Teacher),
            ManagementStaffCount = staff.Count(u => u.Role == UserRole.ManagementStaff),
            AccountantStaffCount = staff.Count(u => u.Role == UserRole.AccountantStaff),
            AdminCount = staff.Count(u => u.Role == UserRole.Admin),
            AverageWorkHoursPerStaff = staff.Count > 0 ? Math.Round(totalWorkHours / staff.Count, 2) : 0,
            PayrollPaidInSelectedRange = totalPayroll,
            PayrollRunApprovedOrPaidCount = processedRuns,
            PayrollRunDraftCount = draftRuns,
            PayrollRunStatusBreakdown =
            [
                new StatusBreakdownItem { Status = PayrollRunStatus.Draft.ToString(), Count = draftRuns, Percentage = payrollRunTotal > 0 ? Math.Round((double)draftRuns / payrollRunTotal * 100, 2) : 0 },
                new StatusBreakdownItem { Status = PayrollRunStatus.Approved.ToString(), Count = payrollRuns.Count(p => p.Status == PayrollRunStatus.Approved), Percentage = payrollRunTotal > 0 ? Math.Round((double)payrollRuns.Count(p => p.Status == PayrollRunStatus.Approved) / payrollRunTotal * 100, 2) : 0 },
                new StatusBreakdownItem { Status = PayrollRunStatus.Paid.ToString(), Count = payrollRuns.Count(p => p.Status == PayrollRunStatus.Paid), Percentage = payrollRunTotal > 0 ? Math.Round((double)payrollRuns.Count(p => p.Status == PayrollRunStatus.Paid) / payrollRunTotal * 100, 2) : 0 }
            ]
        };
    }
}

