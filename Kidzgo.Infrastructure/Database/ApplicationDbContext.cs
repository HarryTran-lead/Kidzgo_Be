using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
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
using Kidzgo.Domain.LearningTickets;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kidzgo.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IPublisher publisher)
    : DbContext(options), IDbContext
{
    // Users
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<ParentPinResetToken> ParentPinResetTokens => Set<ParentPinResetToken>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ParentStudentLink> ParentStudentLinks => Set<ParentStudentLink>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    // Schools
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    // Classes
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassEnrollment> ClassEnrollments => Set<ClassEnrollment>();
    public DbSet<ClassScheduleSegment> ClassScheduleSegments => Set<ClassScheduleSegment>();
    public DbSet<ClassEnrollmentScheduleSegment> ClassEnrollmentScheduleSegments => Set<ClassEnrollmentScheduleSegment>();
    public DbSet<PauseEnrollmentRequest> PauseEnrollmentRequests => Set<PauseEnrollmentRequest>();
    public DbSet<PauseEnrollmentRequestHistory> PauseEnrollmentRequestHistories => Set<PauseEnrollmentRequestHistory>();
    public DbSet<PauseEnrollmentSettings> PauseEnrollmentSettings => Set<PauseEnrollmentSettings>();

    // CRM
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<LeadChild> LeadChildren => Set<LeadChild>();
    public DbSet<PlacementTest> PlacementTests => Set<PlacementTest>();
    public DbSet<ProgramProgressionRule> ProgramProgressionRules => Set<ProgramProgressionRule>();
    public DbSet<ProgramProgressionAssessment> ProgramProgressionAssessments => Set<ProgramProgressionAssessment>();
    public DbSet<ProgramProgressionSchedule> ProgramProgressionSchedules => Set<ProgramProgressionSchedule>();
    public DbSet<ProgramProgressionScheduleParticipant> ProgramProgressionScheduleParticipants => Set<ProgramProgressionScheduleParticipant>();

    // Exams
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
    public DbSet<ExamSubmission> ExamSubmissions => Set<ExamSubmission>();
    public DbSet<ExamSubmissionAnswer> ExamSubmissionAnswers => Set<ExamSubmissionAnswer>();
  
    // Finance
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CashbookEntry> CashbookEntries => Set<CashbookEntry>();

    // Gamification
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<MissionProgress> MissionProgresses => Set<MissionProgress>();
    public DbSet<RewardRedemption> RewardRedemptions => Set<RewardRedemption>();
    public DbSet<RewardStoreItem> RewardStoreItems => Set<RewardStoreItem>();
    public DbSet<StarTransaction> StarTransactions => Set<StarTransaction>();
    public DbSet<StudentLevel> StudentLevels => Set<StudentLevel>();
    public DbSet<AttendanceStreak> AttendanceStreaks => Set<AttendanceStreak>();
    public DbSet<GamificationSettings> GamificationSettings => Set<GamificationSettings>();
    public DbSet<MissionRewardRule> MissionRewardRules => Set<MissionRewardRule>();

    // Lesson Plans
    public DbSet<HomeworkAssignment> HomeworkAssignments => Set<HomeworkAssignment>();
    public DbSet<HomeworkStudent> HomeworkStudents => Set<HomeworkStudent>();
    public DbSet<HomeworkSubmissionAttempt> HomeworkSubmissionAttempts => Set<HomeworkSubmissionAttempt>();
    public DbSet<HomeworkQuestion> HomeworkQuestions => Set<HomeworkQuestion>();
    public DbSet<QuestionBankItem> QuestionBankItems => Set<QuestionBankItem>();
    public DbSet<LessonPlan> LessonPlans => Set<LessonPlan>();
    public DbSet<LessonPlanTemplate> LessonPlanTemplates => Set<LessonPlanTemplate>();

    // Media
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<FaqCategory> FaqCategories => Set<FaqCategory>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    // Payroll
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<PayrollPayment> PayrollPayments => Set<PayrollPayment>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<SessionRole> SessionRoles => Set<SessionRole>();
    public DbSet<ShiftAttendance> ShiftAttendances => Set<ShiftAttendance>();
    public DbSet<MonthlyWorkHours> MonthlyWorkHours => Set<MonthlyWorkHours>();
    public DbSet<TeacherCompensationSettings> TeacherCompensationSettings => Set<TeacherCompensationSettings>();

    // Programs
    public DbSet<Program> Programs => Set<Program>();
    public DbSet<BranchProgram> BranchPrograms => Set<BranchProgram>();
    public DbSet<ExtracurricularProgram> ExtracurricularPrograms => Set<ExtracurricularProgram>();
    public DbSet<ProgramLeavePolicy> ProgramLeavePolicies => Set<ProgramLeavePolicy>();
    public DbSet<TuitionPlan> TuitionPlans => Set<TuitionPlan>();

    // Teaching materials
    public DbSet<TeachingMaterial> TeachingMaterials => Set<TeachingMaterial>();
    public DbSet<TeachingMaterialSlide> TeachingMaterialSlides => Set<TeachingMaterialSlide>();
    public DbSet<TeachingMaterialViewProgress> TeachingMaterialViewProgresses => Set<TeachingMaterialViewProgress>();
    public DbSet<TeachingMaterialBookmark> TeachingMaterialBookmarks => Set<TeachingMaterialBookmark>();
    public DbSet<TeachingMaterialAnnotation> TeachingMaterialAnnotations => Set<TeachingMaterialAnnotation>();

    // Registrations
    public DbSet<RegistrationDiscountCampaign> RegistrationDiscountCampaigns => Set<RegistrationDiscountCampaign>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<EnrollmentConfirmationPdf> EnrollmentConfirmationPdfs => Set<EnrollmentConfirmationPdf>();
    public DbSet<EnrollmentConfirmationPaymentSetting> EnrollmentConfirmationPaymentSettings => Set<EnrollmentConfirmationPaymentSetting>();
    public DbSet<LandingPageSettings> LandingPageSettings => Set<LandingPageSettings>();

    // Reports
    public DbSet<MonthlyReportJob> MonthlyReportJobs => Set<MonthlyReportJob>();
    public DbSet<MonthlyReportData> MonthlyReportData => Set<MonthlyReportData>();
    public DbSet<ReportComment> ReportComments => Set<ReportComment>();
    public DbSet<ReportRequest> ReportRequests => Set<ReportRequest>();
    public DbSet<StudentMonthlyReport> StudentMonthlyReports => Set<StudentMonthlyReport>();
    public DbSet<SessionReport> SessionReports => Set<SessionReport>();

    // Sessions
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<MakeupAllocation> MakeupAllocations => Set<MakeupAllocation>();
    public DbSet<MakeupCredit> MakeupCredits => Set<MakeupCredit>();
    public DbSet<MakeupSettings> MakeupSettings => Set<MakeupSettings>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<StudentSessionAssignment> StudentSessionAssignments => Set<StudentSessionAssignment>();

    // Tickets
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<LearningTicketItem> LearningTicketItems => Set<LearningTicketItem>();
    public DbSet<LearningTicketLedger> LearningTicketLedgers => Set<LearningTicketLedger>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);

        // Keep DateTime columns canonicalized as UTC before they hit PostgreSQL.
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => VietnamTime.EnsureUtcKind(v),
            v => VietnamTime.EnsureUtcKind(v));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var dateTimeProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in dateTimeProperties)
            {
                modelBuilder.Entity(entityType.ClrType).Property(property.Name).HasConversion(dateTimeConverter);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents;
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }
}
