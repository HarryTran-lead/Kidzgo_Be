namespace Kidzgo.Application.Sessions.GetSessionById;

public sealed class GetSessionByIdResponse
{
    public SessionDetailDto Session { get; init; } = null!;
}

public sealed class SessionDetailDto
{
    public Guid Id { get; init; }
    public string? Color { get; init; }
    public Guid ClassId { get; init; }
    public Guid? ModuleId { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public int? SessionIndexInModule { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public DateTime PlannedDatetime { get; init; }
    public DateTime? ActualDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public string ParticipationType { get; init; } = null!;
    public string SectionType { get; init; } = null!;
    public Guid? SlotTypeId { get; init; }
    public string? SlotTypeCode { get; init; }
    public string Status { get; init; } = null!;
    public Guid? PlannedRoomId { get; init; }
    public string? PlannedRoomName { get; init; }
    public Guid? ActualRoomId { get; init; }
    public string? ActualRoomName { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public string? PlannedTeacherName { get; init; }
    public Guid? ActualTeacherId { get; init; }
    public string? ActualTeacherName { get; init; }
    public Guid? PlannedAssistantId { get; init; }
    public string? PlannedAssistantName { get; init; }
    public Guid? ActualAssistantId { get; init; }
    public string? ActualAssistantName { get; init; }
    public Guid? LessonPlanId { get; init; }
    public string? LessonPlanLink { get; init; }
    public AttendanceSummaryDto AttendanceSummary { get; init; } = null!;
}

public sealed class AttendanceSummaryDto
{
    public int TotalStudents { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int MakeupCount { get; init; }
    public int NotMarkedCount { get; init; }
}

