using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Domain.Users;

public class User : Entity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public string? Username  { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PinHash { get; set; }
    public string? AvatarUrl { get; set; }  // Avatar cho user
    public string? AvatarMimeType { get; set; }  // MIME type của avatar
    public long? AvatarFileSize { get; set; }  // Kích thước avatar (bytes)
    public TeacherCompensationType? TeacherCompensationType { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Branch? Branch { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Profile> Profiles { get; set; } = new List<Profile>();
    public ICollection<Class> MainTeacherClasses { get; set; } = new List<Class>();
    public ICollection<Class> AssistantTeacherClasses { get; set; } = new List<Class>();
    public ICollection<Session> PlannedTeacherSessions { get; set; } = new List<Session>();
    public ICollection<Session> PlannedAssistantSessions { get; set; } = new List<Session>();
    public ICollection<Session> ActualTeacherSessions { get; set; } = new List<Session>();
    public ICollection<Session> ActualAssistantSessions { get; set; } = new List<Session>();
    public ICollection<LeaveRequest> ApprovedLeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<Attendance> MarkedAttendances { get; set; } = new List<Attendance>();
    public ICollection<MakeupAllocation> AssignedMakeupAllocations { get; set; } = new List<MakeupAllocation>();
    public ICollection<LessonPlanTemplate> CreatedLessonPlanTemplates { get; set; } = new List<LessonPlanTemplate>();
    public ICollection<Syllabus> CreatedSyllabuses { get; set; } = new List<Syllabus>();
    public ICollection<Sessions.TeachingLog> TeachingLogs { get; set; } = new List<Sessions.TeachingLog>();
    public ICollection<LessonPlan> SubmittedLessonPlans { get; set; } = new List<LessonPlan>();
    public ICollection<HomeworkAssignment> CreatedHomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
    public ICollection<Exam> CreatedExams { get; set; } = new List<Exam>();
    public ICollection<ExamResult> GradedExamResults { get; set; } = new List<ExamResult>();
    public ICollection<StudentMonthlyReport> SubmittedReports { get; set; } = new List<StudentMonthlyReport>();
    public ICollection<StudentMonthlyReport> ReviewedReports { get; set; } = new List<StudentMonthlyReport>();
    public ICollection<ReportTemplate> CreatedReportTemplates { get; set; } = new List<ReportTemplate>();
    public ICollection<ReportRun> GeneratedReportRuns { get; set; } = new List<ReportRun>();
    public ICollection<RiskRuleConfig> UpdatedRiskRuleConfigs { get; set; } = new List<RiskRuleConfig>();
    public ICollection<ReportComment> ReportComments { get; set; } = new List<ReportComment>();
    public ICollection<Mission> CreatedMissions { get; set; } = new List<Mission>();
    public ICollection<MissionProgress> VerifiedMissionProgresses { get; set; } = new List<MissionProgress>();
    public ICollection<StarTransaction> CreatedStarTransactions { get; set; } = new List<StarTransaction>();
    public ICollection<RewardRedemption> HandledRewardRedemptions { get; set; } = new List<RewardRedemption>();
    public ICollection<Lead> OwnedLeads { get; set; } = new List<Lead>();
    public ICollection<PlacementTest> InvigilatedPlacementTests { get; set; } = new List<PlacementTest>();
    public ICollection<LeadActivity> CreatedLeadActivities { get; set; } = new List<LeadActivity>();
    public ICollection<MediaAsset> UploadedMediaAssets { get; set; } = new List<MediaAsset>();
    public ICollection<Invoice> IssuedInvoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> ConfirmedPayments { get; set; } = new List<Payment>();
    public ICollection<CashbookEntry> CreatedCashbookEntries { get; set; } = new List<CashbookEntry>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<ShiftAttendance> ShiftAttendances { get; set; } = new List<ShiftAttendance>();
    public ICollection<ShiftAttendance> ApprovedShiftAttendances { get; set; } = new List<ShiftAttendance>();
    public ICollection<SessionRole> SessionRoles { get; set; } = new List<SessionRole>();
    public ICollection<PayrollRun> ApprovedPayrollRuns { get; set; } = new List<PayrollRun>();
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
    public ICollection<PayrollPayment> PayrollPayments { get; set; } = new List<PayrollPayment>();
    public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
    public ICollection<Ticket> OpenedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
