using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.Shared;

public sealed class ProgramProgressionRuleDto
{
    public Guid Id { get; init; }
    public Guid SourceLevelId { get; init; }
    public string SourceLevelName { get; init; } = null!;
    public Guid? TargetLevelId { get; init; }
    public string? TargetLevelName { get; init; }
    public Guid SourceProgramId { get; init; }
    public string SourceProgramName { get; init; } = null!;
    public Guid? TargetProgramId { get; init; }
    public string? TargetProgramName { get; init; }
    public ProgramProgressionMethod Method { get; init; }
    public int? MinimumShieldCount { get; init; }
    public int? MinimumSkillShieldCount { get; init; }
    public decimal? MinimumOverallScore { get; init; }
    public bool CarryOverRemainingSessions { get; init; }
    public bool StopCurrentEnrollmentOnApproval { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<ProgramProgressionShieldRange> ShieldMappings { get; init; } = Array.Empty<ProgramProgressionShieldRange>();
    public IReadOnlyList<ProgramProgressionClassificationBand> ClassificationBands { get; init; } = Array.Empty<ProgramProgressionClassificationBand>();
    public IReadOnlyList<PracticeTestScoreMapping> PracticeTestScoreMappings { get; init; } = Array.Empty<PracticeTestScoreMapping>();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class ProgramProgressionAssessmentDto
{
    public Guid Id { get; init; }
    public Guid RuleId { get; init; }
    public Guid? ScheduleParticipantId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid SourceProgramId { get; init; }
    public string SourceProgramName { get; init; } = null!;
    public Guid SourceLevelId { get; init; }
    public string SourceLevelName { get; init; } = null!;
    public Guid? TargetProgramId { get; init; }
    public string? TargetProgramName { get; init; }
    public Guid? TargetLevelId { get; init; }
    public string? TargetLevelName { get; init; }
    public Guid SourceRegistrationId { get; init; }
    public Guid? SourceEnrollmentId { get; init; }
    public Guid? SourceClassId { get; init; }
    public string? SourceClassCode { get; init; }
    public string? SourceClassTitle { get; init; }
    public DateTime AssessmentDate { get; init; }
    public ProgramProgressionMethod Method { get; init; }
    public ProgramProgressionAssessmentStatus Status { get; init; }
    public bool? PassedInClass { get; init; }
    public int? ListeningPracticeScore { get; init; }
    public int? SpeakingPracticeScore { get; init; }
    public int? ReadingPracticeScore { get; init; }
    public int? WritingPracticeScore { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingWritingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public decimal? OverallScore { get; init; }
    public int? ListeningShieldCount { get; init; }
    public int? SpeakingShieldCount { get; init; }
    public int? ReadingWritingShieldCount { get; init; }
    public int? TotalShieldCount { get; init; }
    public bool IsEligible { get; init; }
    public string? ResultBand { get; init; }
    public string? ResultLevel { get; init; }
    public string? Comment { get; init; }
    public IReadOnlyList<string> AttachmentUrls { get; init; } = Array.Empty<string>();
    public Guid? RecordedBy { get; init; }
    public Guid? ApprovedBy { get; init; }
    public Guid? ApprovedTuitionPlanId { get; init; }
    public string? ApprovedTuitionPlanName { get; init; }
    public Guid? GeneratedRegistrationId { get; init; }
    public string? ApprovalNote { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class ProgramProgressionScheduleParticipantDto
{
    public Guid Id { get; init; }
    public Guid ScheduleId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public string? StudentAvatarUrl { get; init; }
    public Guid SourceRegistrationId { get; init; }
    public Guid? SourceEnrollmentId { get; init; }
    public ProgramProgressionScheduleParticipantStatus Status { get; init; }
    public Guid? AssessmentId { get; init; }
    public ProgramProgressionAssessmentStatus? AssessmentStatus { get; init; }
    public bool? IsEligible { get; init; }
    public string? ResultBand { get; init; }
    public string? ResultLevel { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class ProgramProgressionScheduleDto
{
    public Guid Id { get; init; }
    public Guid SourceClassId { get; init; }
    public string SourceClassCode { get; init; } = null!;
    public string SourceClassTitle { get; init; } = null!;
    public Guid SourceProgramId { get; init; }
    public string SourceProgramName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public Guid AssignedTeacherUserId { get; init; }
    public string AssignedTeacherName { get; init; } = null!;
    public ProgramProgressionScheduleStatus Status { get; init; }
    public string? Notes { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByUserName { get; init; } = null!;
    public int ParticipantCount { get; init; }
    public int ScheduledParticipantCount { get; init; }
    public int CompletedParticipantCount { get; init; }
    public int NoShowParticipantCount { get; init; }
    public int CancelledParticipantCount { get; init; }
    public IReadOnlyList<ProgramProgressionScheduleParticipantDto> Participants { get; init; } = Array.Empty<ProgramProgressionScheduleParticipantDto>();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

internal static class ProgramProgressionDtoMapper
{
    public static ProgramProgressionRuleDto ToDto(this ProgramProgressionRule rule)
    {
        return new ProgramProgressionRuleDto
        {
            Id = rule.Id,
            SourceLevelId = rule.SourceLevelId,
            SourceLevelName = rule.SourceLevel?.Name ?? string.Empty,
            TargetLevelId = rule.TargetLevelId,
            TargetLevelName = rule.TargetLevel?.Name,
            SourceProgramId = rule.SourceProgramId,
            SourceProgramName = rule.SourceProgram.Name,
            TargetProgramId = rule.TargetProgramId,
            TargetProgramName = rule.TargetProgram?.Name,
            Method = rule.Method,
            MinimumShieldCount = rule.MinimumShieldCount,
            MinimumSkillShieldCount = rule.MinimumSkillShieldCount,
            MinimumOverallScore = rule.MinimumOverallScore,
            CarryOverRemainingSessions = rule.CarryOverRemainingSessions,
            StopCurrentEnrollmentOnApproval = rule.StopCurrentEnrollmentOnApproval,
            IsActive = rule.IsActive,
            Notes = rule.Notes,
            ShieldMappings = ProgramProgressionRuleDefinition.DeserializeShieldMappings(rule.ShieldMappingJson),
            ClassificationBands = ProgramProgressionRuleDefinition.DeserializeClassificationBands(rule.ClassificationBandsJson),
            PracticeTestScoreMappings = ProgramProgressionRuleDefinition.DeserializePracticeTestScoreMappings(rule.PracticeTestScoreMappingsJson),
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }

    public static ProgramProgressionAssessmentDto ToDto(this ProgramProgressionAssessment assessment)
    {
        var sourceClass = assessment.ScheduleParticipant?.Schedule.SourceClass
            ?? assessment.SourceEnrollment?.Class;
        var sourceLevelId = assessment.SourceLevelId;
        var sourceLevelName = assessment.SourceLevel?.Name
            ?? assessment.SourceRegistration?.Level?.Name
            ?? string.Empty;
        var targetLevelId = assessment.TargetLevelId
            ?? assessment.GeneratedRegistration?.LevelId
            ?? assessment.ApprovedTuitionPlan?.LevelId;
        var targetLevelName = assessment.TargetLevel?.Name
            ?? assessment.GeneratedRegistration?.Level?.Name
            ?? assessment.ApprovedTuitionPlan?.Level?.Name;

        return new ProgramProgressionAssessmentDto
        {
            Id = assessment.Id,
            RuleId = assessment.RuleId,
            ScheduleParticipantId = assessment.ScheduleParticipantId,
            StudentProfileId = assessment.StudentProfileId,
            StudentName = assessment.StudentProfile.DisplayName,
            SourceProgramId = assessment.SourceProgramId,
            SourceProgramName = assessment.SourceProgram.Name,
            SourceLevelId = sourceLevelId,
            SourceLevelName = sourceLevelName,
            TargetProgramId = assessment.TargetProgramId,
            TargetProgramName = assessment.TargetProgram?.Name,
            TargetLevelId = targetLevelId,
            TargetLevelName = targetLevelName,
            SourceRegistrationId = assessment.SourceRegistrationId,
            SourceEnrollmentId = assessment.SourceEnrollmentId,
            SourceClassId = sourceClass?.Id,
            SourceClassCode = sourceClass?.Code,
            SourceClassTitle = sourceClass?.Title,
            AssessmentDate = assessment.AssessmentDate,
            Method = assessment.Method,
            Status = assessment.Status,
            PassedInClass = assessment.PassedInClass,
            ListeningPracticeScore = assessment.ListeningPracticeScore,
            SpeakingPracticeScore = assessment.SpeakingPracticeScore,
            ReadingPracticeScore = assessment.ReadingPracticeScore,
            WritingPracticeScore = assessment.WritingPracticeScore,
            ListeningScore = assessment.ListeningScore,
            SpeakingScore = assessment.SpeakingScore,
            ReadingWritingScore = assessment.ReadingWritingScore,
            ReadingScore = assessment.ReadingScore,
            WritingScore = assessment.WritingScore,
            OverallScore = assessment.OverallScore,
            ListeningShieldCount = assessment.ListeningShieldCount,
            SpeakingShieldCount = assessment.SpeakingShieldCount,
            ReadingWritingShieldCount = assessment.ReadingWritingShieldCount,
            TotalShieldCount = assessment.TotalShieldCount,
            IsEligible = assessment.IsEligible,
            ResultBand = assessment.ResultBand,
            ResultLevel = assessment.ResultLevel,
            Comment = assessment.Comment,
            AttachmentUrls = ProgramProgressionAttachmentUrlHelper.Parse(assessment.AttachmentUrls),
            RecordedBy = assessment.RecordedBy,
            ApprovedBy = assessment.ApprovedBy,
            ApprovedTuitionPlanId = assessment.ApprovedTuitionPlanId,
            ApprovedTuitionPlanName = assessment.ApprovedTuitionPlan?.Name,
            GeneratedRegistrationId = assessment.GeneratedRegistrationId,
            ApprovalNote = assessment.ApprovalNote,
            ApprovedAt = assessment.ApprovedAt,
            CreatedAt = assessment.CreatedAt,
            UpdatedAt = assessment.UpdatedAt
        };
    }

    public static ProgramProgressionScheduleDto ToDto(
        this ProgramProgressionSchedule schedule,
        Func<ProgramProgressionScheduleParticipant, bool>? participantFilter = null)
    {
        var participants = participantFilter is null
            ? schedule.Participants.ToList()
            : schedule.Participants.Where(participantFilter).ToList();

        return new ProgramProgressionScheduleDto
        {
            Id = schedule.Id,
            SourceClassId = schedule.SourceClassId,
            SourceClassCode = schedule.SourceClass.Code,
            SourceClassTitle = schedule.SourceClass.Title,
            SourceProgramId = schedule.SourceProgramId,
            SourceProgramName = schedule.SourceProgram.Name,
            BranchId = schedule.BranchId,
            BranchName = schedule.Branch.Name,
            ScheduledAt = schedule.ScheduledAt,
            DurationMinutes = schedule.DurationMinutes,
            RoomId = schedule.RoomId,
            RoomName = schedule.Room?.Name,
            AssignedTeacherUserId = schedule.AssignedTeacherUserId,
            AssignedTeacherName = schedule.AssignedTeacherUser.Name ?? schedule.AssignedTeacherUser.Email,
            Status = schedule.Status,
            Notes = schedule.Notes,
            CreatedByUserId = schedule.CreatedByUserId,
            CreatedByUserName = schedule.CreatedByUser.Name ?? schedule.CreatedByUser.Email,
            ParticipantCount = participants.Count,
            ScheduledParticipantCount = participants.Count(p => p.Status == ProgramProgressionScheduleParticipantStatus.Scheduled),
            CompletedParticipantCount = participants.Count(p => p.Status == ProgramProgressionScheduleParticipantStatus.Completed),
            NoShowParticipantCount = participants.Count(p => p.Status == ProgramProgressionScheduleParticipantStatus.NoShow),
            CancelledParticipantCount = participants.Count(p => p.Status == ProgramProgressionScheduleParticipantStatus.Cancelled),
            Participants = participants
                .OrderBy(p => p.StudentProfile.DisplayName)
                .Select(p => p.ToDto())
                .ToList(),
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }

    public static ProgramProgressionScheduleParticipantDto ToDto(this ProgramProgressionScheduleParticipant participant)
    {
        return new ProgramProgressionScheduleParticipantDto
        {
            Id = participant.Id,
            ScheduleId = participant.ScheduleId,
            StudentProfileId = participant.StudentProfileId,
            StudentName = participant.StudentProfile.DisplayName,
            StudentAvatarUrl = participant.StudentProfile.AvatarUrl,
            SourceRegistrationId = participant.SourceRegistrationId,
            SourceEnrollmentId = participant.SourceEnrollmentId,
            Status = participant.Status,
            AssessmentId = participant.Assessment?.Id,
            AssessmentStatus = participant.Assessment?.Status,
            IsEligible = participant.Assessment?.IsEligible,
            ResultBand = participant.Assessment?.ResultBand,
            ResultLevel = participant.Assessment?.ResultLevel,
            CreatedAt = participant.CreatedAt,
            UpdatedAt = participant.UpdatedAt
        };
    }
}
