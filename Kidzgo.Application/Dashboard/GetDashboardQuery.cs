using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Dashboard;

public sealed class GetDashboardQuery : IQuery<DashboardResponse>
{
    public Guid? BranchId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed class DashboardResponse
{
    // Academic Dashboard
    public AttendanceStats Attendance { get; init; } = new();
    public HomeworkStats Homework { get; init; } = new();
    public LeaveStats Leave { get; init; } = new();
    public MakeupCreditStats MakeupCredits { get; init; } = new();

    // Student & Enrollment Stats
    public StudentStats Students { get; init; } = new();
    public EnrollmentStats Enrollments { get; init; } = new();

    // Lead & CRM Stats
    public LeadStats Leads { get; init; } = new();
    public PlacementTestStats PlacementTests { get; init; } = new();

    // Financial Dashboard
    public FinanceStats Finance { get; init; } = new();

    // HR Dashboard
    public HrStats HumanResources { get; init; } = new();
}

public sealed class AttendanceStats
{
    // Legacy fields
    public int TotalSessions { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int LateCount { get; init; }
    public double AttendanceRate { get; init; }

    // Detailed fields
    public int TotalAttendanceRecords { get; init; }
    public int UniqueSessionCount { get; init; }
    public double PresentRate { get; init; }
    public double AbsentRate { get; init; }
    public double MakeupRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class HomeworkStats
{
    // Legacy fields
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Submitted { get; init; }
    public int Graded { get; init; }
    public int Overdue { get; init; }

    // Detailed fields
    public int TotalHomeworkSubmissions { get; init; }
    public int AssignedCount { get; init; }
    public int SubmittedCount { get; init; }
    public int GradedCount { get; init; }
    public int LateCount { get; init; }
    public int MissingCount { get; init; }
    public int OverdueCount { get; init; }
    public double SubmissionRate { get; init; }
    public double GradedRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class LeaveStats
{
    // Legacy fields
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }

    // Detailed fields
    public int TotalRequests { get; init; }
    public int PendingRequests { get; init; }
    public int ApprovedRequests { get; init; }
    public int RejectedRequests { get; init; }
    public double ApprovalRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class MakeupCreditStats
{
    // Legacy fields
    public int TotalCredits { get; init; }
    public int UsedCredits { get; init; }
    public int AvailableCredits { get; init; }
    public int TransferredCredits { get; init; }

    // Detailed fields
    public int TotalCreditsIssued { get; init; }
    public int UsedCreditsCount { get; init; }
    public int AvailableCreditsCount { get; init; }
    public int TransferredCreditsCount { get; init; }
    public int ExpiredCreditsCount { get; init; }
    public double UtilizationRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class StudentStats
{
    // Legacy fields
    public int Total { get; init; }
    public int Active { get; init; }
    public int Inactive { get; init; }
    public int NewThisMonth { get; init; }

    // Detailed fields
    public int TotalStudents { get; init; }
    public int ActiveStudents { get; init; }
    public int InactiveStudents { get; init; }
    public int NewStudentsThisMonth { get; init; }
    public int NewStudentsInSelectedRange { get; init; }
    public double ActiveStudentRate { get; init; }
}

public sealed class EnrollmentStats
{
    // Legacy fields (backward compatibility)
    public int Total { get; init; }
    public int Active { get; init; }
    public int Paused { get; init; }
    public int Dropped { get; init; }
    public int NewThisMonth { get; init; }

    // Clearer fields for frontend display
    public int TotalEnrollments { get; init; }
    public int ActiveEnrollments { get; init; }
    public int PausedEnrollments { get; init; }
    public int DroppedEnrollments { get; init; }
    public int NewEnrollmentsThisMonth { get; init; }
    public int NewEnrollmentsInSelectedRange { get; init; }
    public double ActiveEnrollmentRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class LeadStats
{
    // Legacy fields
    public int Total { get; init; }
    public int New { get; init; }
    public int Contacted { get; init; }
    public int Qualified { get; init; }
    public int Enrolled { get; init; }
    public int NoShow { get; init; }
    public int Lost { get; init; }
    public double ConversionRate { get; init; }
    public int TotalTouchCount { get; init; }

    // Detailed fields
    public int TotalLeads { get; init; }
    public int NewLeads { get; init; }
    public int ContactedLeads { get; init; }
    public int QualifiedLeads { get; init; }
    public int EnrolledLeads { get; init; }
    public int LostLeads { get; init; }
    public int NoShowLeads { get; init; }
    public double QualificationRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class PlacementTestStats
{
    // Legacy fields
    public int Total { get; init; }
    public int Scheduled { get; init; }
    public int Completed { get; init; }
    public int NoShow { get; init; }
    public int Enrolled { get; init; }

    // Detailed fields
    public int TotalTests { get; init; }
    public int ScheduledTests { get; init; }
    public int CompletedTests { get; init; }
    public int NoShowTests { get; init; }
    public int CancelledTests { get; init; }
    public double CompletionRate { get; init; }
    public double NoShowRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> StatusBreakdown { get; init; } = [];
}

public sealed class FinanceStats
{
    // Legacy fields
    public decimal TotalRevenue { get; init; }
    public decimal MonthRevenue { get; init; }
    public decimal PayOSRevenue { get; init; }
    public decimal CashRevenue { get; init; }
    public decimal OutstandingDebt { get; init; }
    public int PaidInvoices { get; init; }
    public int PendingInvoices { get; init; }
    public int OverdueInvoices { get; init; }

    // Detailed fields
    public decimal TotalBilledAmount { get; init; }
    public decimal TotalCollectedAmount { get; init; }
    public decimal CollectedInSelectedRange { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public double CollectionRate { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> InvoiceStatusBreakdown { get; init; } = [];
}

public sealed class HrStats
{
    // Legacy fields
    public int TotalStaff { get; init; }
    public double TotalWorkHours { get; init; }
    public decimal TotalPayroll { get; init; }
    public int PayrollProcessed { get; init; }
    public int PayrollPending { get; init; }

    // Detailed fields
    public int TeacherCount { get; init; }
    public int ManagementStaffCount { get; init; }
    public int AccountantStaffCount { get; init; }
    public int AdminCount { get; init; }
    public double AverageWorkHoursPerStaff { get; init; }
    public decimal PayrollPaidInSelectedRange { get; init; }
    public int PayrollRunApprovedOrPaidCount { get; init; }
    public int PayrollRunDraftCount { get; init; }
    public IReadOnlyCollection<StatusBreakdownItem> PayrollRunStatusBreakdown { get; init; } = [];
}

public sealed class StatusBreakdownItem
{
    public string Status { get; init; } = null!;
    public int Count { get; init; }
    public double Percentage { get; init; }
}
