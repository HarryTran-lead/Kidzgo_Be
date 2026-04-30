using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.LearningHistory.GetLearningHistory;

public sealed class GetLearningHistoryResponse
{
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public LearningHistorySummaryDto Summary { get; init; } = null!;
    public List<LearningHistoryRegistrationDto> Registrations { get; init; } = new();
    public List<LearningHistoryEnrollmentDto> Enrollments { get; init; } = new();
    public Page<LearningHistorySessionDto> Sessions { get; init; } = new(new List<LearningHistorySessionDto>(), 0, 1, 20);
    public Page<LearningHistoryMissionDto> Missions { get; init; } = new(new List<LearningHistoryMissionDto>(), 0, 1, 20);
}

public sealed class LearningHistorySummaryDto
{
    public int TotalRegistrations { get; init; }
    public int CompletedRegistrations { get; init; }
    public int TotalPurchasedSessions { get; init; }
    public int TotalUsedSessions { get; init; }
    public int TotalRemainingSessions { get; init; }
    public int TotalEnrollments { get; init; }
    public int CompletedEnrollments { get; init; }
    public int TotalSessionRecords { get; init; }
    public int PresentSessions { get; init; }
    public int AbsentSessions { get; init; }
    public int MakeupSessions { get; init; }
    public int TotalMissions { get; init; }
    public int CompletedMissions { get; init; }
}

public sealed class LearningHistoryRegistrationDto
{
    public Guid Id { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string Status { get; init; } = null!;
    public string? OperationType { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid? SecondaryProgramId { get; init; }
    public string? SecondaryProgramName { get; init; }
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public Guid? SecondaryClassId { get; init; }
    public string? SecondaryClassName { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public DateTime? ActualStartDate { get; init; }
    public int TotalSessions { get; init; }
    public int UsedSessions { get; init; }
    public int RemainingSessions { get; init; }
    public Guid? OriginalRegistrationId { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class LearningHistoryEnrollmentDto
{
    public Guid Id { get; init; }
    public Guid? RegistrationId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public string Track { get; init; } = null!;
    public DateOnly EnrollDate { get; init; }
    public EnrollmentStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class LearningHistorySessionDto
{
    public Guid SessionId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime PlannedDatetime { get; init; }
    public DateTime? ActualDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public string SessionStatus { get; init; } = null!;
    public Guid? RegistrationId { get; init; }
    public string? Track { get; init; }
    public bool IsMakeup { get; init; }
    public string? AttendanceStatus { get; init; }
    public string? AbsenceType { get; init; }
    public DateTime? AttendanceMarkedAt { get; init; }
    public string? AttendanceNote { get; init; }
    public Guid? TeacherId { get; init; }
    public string? TeacherName { get; init; }
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
}

public sealed class LearningHistoryMissionDto
{
    public Guid Id { get; init; }
    public Guid MissionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string MissionType { get; init; } = null!;
    public string ProgressMode { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal? ProgressValue { get; init; }
    public int? TotalRequired { get; init; }
    public decimal ProgressPercentage { get; init; }
    public int? RewardStars { get; init; }
    public int? RewardExp { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
