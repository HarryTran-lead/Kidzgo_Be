namespace Kidzgo.Application.Sessions.GetStudentTimetable;

public sealed class GetStudentTimetableResponse
{
    public List<TimetableItemDto> Sessions { get; init; } = new();
}

public sealed class TimetableItemDto
{
    public Guid Id { get; init; }
    public Guid? StudentProfileId { get; init; }
    public string? StudentDisplayName { get; init; }
    public string? StudentAvatarUrl { get; init; }
    public string? Color { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime PlannedDatetime { get; init; }
    public DateTime? ActualDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public string? ParticipationType { get; init; }
    public string? SectionType { get; init; }
    public Guid? SlotTypeId { get; init; }
    public string? SlotTypeCode { get; init; }
    public string? Status { get; init; }
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
    public Guid? LessonPlanId { get; init; }
    public string? LessonPlanLink { get; init; }
    public Guid? RegistrationId { get; init; }
    public string? Track { get; init; }
    public bool IsMakeup { get; init; }

    // Attendance information
    public string? AttendanceStatus { get; init; }
    public string? AbsenceType { get; init; }
    public DateTime? AttendanceMarkedAt { get; init; }
}

