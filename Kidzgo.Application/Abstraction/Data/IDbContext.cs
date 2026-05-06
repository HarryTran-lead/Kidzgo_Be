using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Faqs;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.Website;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.Abstraction.Data;

public interface IDbContext
{
    DatabaseFacade Database { get; }

    // Users
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<ParentPinResetToken> ParentPinResetTokens { get; }

    DbSet<Profile> Profiles { get; }
    DbSet<ParentStudentLink> ParentStudentLinks { get; }
    DbSet<DeviceToken> DeviceTokens { get; }

    // Schools
    DbSet<Branch> Branches { get; }
    DbSet<Classroom> Classrooms { get; }
    DbSet<Holiday> Holidays { get; }

    // Classes
    DbSet<Class> Classes { get; }
    DbSet<ClassEnrollment> ClassEnrollments { get; }
    DbSet<ClassScheduleSegment> ClassScheduleSegments { get; }
    DbSet<ClassEnrollmentScheduleSegment> ClassEnrollmentScheduleSegments { get; }
    DbSet<PauseEnrollmentRequest> PauseEnrollmentRequests { get; }
    DbSet<PauseEnrollmentRequestHistory> PauseEnrollmentRequestHistories { get; }
    DbSet<PauseEnrollmentSettings> PauseEnrollmentSettings { get; }

    // CRM
    DbSet<Lead> Leads { get; }
    DbSet<LeadActivity> LeadActivities { get; }
    DbSet<LeadChild> LeadChildren { get; }
    DbSet<PlacementTest> PlacementTests { get; }
    DbSet<ProgramProgressionRule> ProgramProgressionRules { get; }
    DbSet<ProgramProgressionAssessment> ProgramProgressionAssessments { get; }
    DbSet<ProgramProgressionSchedule> ProgramProgressionSchedules { get; }
    DbSet<ProgramProgressionScheduleParticipant> ProgramProgressionScheduleParticipants { get; }

    // Exams
    DbSet<Exam> Exams { get; }
    DbSet<ExamResult> ExamResults { get; }
    DbSet<ExamQuestion> ExamQuestions { get; }
    DbSet<ExamSubmission> ExamSubmissions { get; }
    DbSet<ExamSubmissionAnswer> ExamSubmissionAnswers { get; }

    // Finance
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }
    DbSet<Payment> Payments { get; }
    DbSet<CashbookEntry> CashbookEntries { get; }

    // Gamification
    DbSet<Mission> Missions { get; }
    DbSet<MissionProgress> MissionProgresses { get; }
    DbSet<RewardRedemption> RewardRedemptions { get; }
    DbSet<RewardStoreItem> RewardStoreItems { get; }
    DbSet<StarTransaction> StarTransactions { get; }
    DbSet<StudentLevel> StudentLevels { get; }
    DbSet<AttendanceStreak> AttendanceStreaks { get; }
    DbSet<GamificationSettings> GamificationSettings { get; }
    DbSet<MissionRewardRule> MissionRewardRules { get; }

    // Lesson Plans
    DbSet<HomeworkAssignment> HomeworkAssignments { get; }
    DbSet<HomeworkQuestion> HomeworkQuestions { get; }
    DbSet<QuestionBankItem> QuestionBankItems { get; }

    DbSet<HomeworkStudent> HomeworkStudents { get; }
    DbSet<HomeworkSubmissionAttempt> HomeworkSubmissionAttempts { get; }
    DbSet<LessonPlan> LessonPlans { get; }
    DbSet<LessonPlanTemplate> LessonPlanTemplates { get; }

    // Media
    DbSet<MediaAsset> MediaAssets { get; }
    DbSet<Blog> Blogs { get; }
    DbSet<FaqCategory> FaqCategories { get; }
    DbSet<FaqItem> FaqItems { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<EmailTemplate> EmailTemplates { get; }

    // Payroll
    DbSet<Contract> Contracts { get; }
    DbSet<PayrollLine> PayrollLines { get; }
    DbSet<PayrollPayment> PayrollPayments { get; }
    DbSet<PayrollRun> PayrollRuns { get; }
    DbSet<SessionRole> SessionRoles { get; }
    DbSet<ShiftAttendance> ShiftAttendances { get; }
    DbSet<MonthlyWorkHours> MonthlyWorkHours { get; }
    DbSet<TeacherCompensationSettings> TeacherCompensationSettings { get; }

    // Programs
    DbSet<Program> Programs { get; }
    DbSet<BranchProgram> BranchPrograms { get; }
    DbSet<ExtracurricularProgram> ExtracurricularPrograms { get; }
    DbSet<ProgramLeavePolicy> ProgramLeavePolicies { get; }
    DbSet<TuitionPlan> TuitionPlans { get; }

    // Teaching materials
    DbSet<TeachingMaterial> TeachingMaterials { get; }
    DbSet<TeachingMaterialSlide> TeachingMaterialSlides { get; }
    DbSet<TeachingMaterialViewProgress> TeachingMaterialViewProgresses { get; }
    DbSet<TeachingMaterialBookmark> TeachingMaterialBookmarks { get; }
    DbSet<TeachingMaterialAnnotation> TeachingMaterialAnnotations { get; }

    // Registrations
    DbSet<RegistrationDiscountCampaign> RegistrationDiscountCampaigns { get; }
    DbSet<Registration> Registrations { get; }
    DbSet<EnrollmentConfirmationPdf> EnrollmentConfirmationPdfs { get; }
    DbSet<EnrollmentConfirmationPaymentSetting> EnrollmentConfirmationPaymentSettings { get; }
    DbSet<LandingPageSettings> LandingPageSettings { get; }

    // Reports
    DbSet<MonthlyReportJob> MonthlyReportJobs { get; }
    DbSet<MonthlyReportData> MonthlyReportData { get; }
    DbSet<ReportComment> ReportComments { get; }
    DbSet<ReportRequest> ReportRequests { get; }
    DbSet<SessionReport> SessionReports { get; }
    DbSet<StudentMonthlyReport> StudentMonthlyReports { get; }

    // Sessions
    DbSet<Domain.Sessions.Attendance> Attendances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<MakeupAllocation> MakeupAllocations { get; }
    DbSet<MakeupCredit> MakeupCredits { get; }
    DbSet<MakeupSettings> MakeupSettings { get; }
    DbSet<Session> Sessions { get; }
    DbSet<StudentSessionAssignment> StudentSessionAssignments { get; }

    // Tickets
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketComment> TicketComments { get; }

    // Audit
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
