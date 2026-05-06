using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Users;
using DomainProgram = Kidzgo.Domain.Programs.Program;

namespace Kidzgo.Domain.ProgramProgressions;

public class ProgramProgressionAssessment : Entity
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public Guid? ScheduleParticipantId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid SourceProgramId { get; set; }
    public Guid? TargetProgramId { get; set; }
    public Guid SourceRegistrationId { get; set; }
    public Guid? SourceEnrollmentId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public ProgramProgressionMethod Method { get; set; }
    public ProgramProgressionAssessmentStatus Status { get; set; }
    public bool? PassedInClass { get; set; }
    public decimal? ListeningScore { get; set; }
    public decimal? SpeakingScore { get; set; }
    public decimal? ReadingWritingScore { get; set; }
    public decimal? ReadingScore { get; set; }
    public decimal? WritingScore { get; set; }
    public decimal? OverallScore { get; set; }
    public int? ListeningShieldCount { get; set; }
    public int? SpeakingShieldCount { get; set; }
    public int? ReadingWritingShieldCount { get; set; }
    public int? TotalShieldCount { get; set; }
    public bool IsEligible { get; set; }
    public string? ResultBand { get; set; }
    public string? ResultLevel { get; set; }
    public string? Comment { get; set; }
    public string? AttachmentUrls { get; set; }
    public Guid? RecordedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public Guid? ApprovedTuitionPlanId { get; set; }
    public Guid? GeneratedRegistrationId { get; set; }
    public string? ApprovalNote { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProgramProgressionRule Rule { get; set; } = null!;
    public ProgramProgressionScheduleParticipant? ScheduleParticipant { get; set; }
    public Profile StudentProfile { get; set; } = null!;
    public DomainProgram SourceProgram { get; set; } = null!;
    public DomainProgram? TargetProgram { get; set; }
    public Registration SourceRegistration { get; set; } = null!;
    public ClassEnrollment? SourceEnrollment { get; set; }
    public User? RecordedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public TuitionPlan? ApprovedTuitionPlan { get; set; }
    public Registration? GeneratedRegistration { get; set; }
}
