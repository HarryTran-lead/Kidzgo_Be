using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.GetSessions;

public sealed class GetSessionsResponse
{
    public Page<SessionListItemDto> Sessions { get; init; } = null!;
}

public sealed class SessionListItemDto
{
    public Guid Id { get; init; }
    public string? Color { get; init; }
    public Guid ClassId { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public Guid? PlannedLessonPlanTemplateId { get; init; }
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
    public string? PlannedLessonTitle { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public string? ActualLessonTitle { get; init; }
    public Guid? TeachingLogId { get; init; }
    public string? TeachingLogStatus { get; init; }
    public string? TeachingProgressStatus { get; init; }
    public string? ActualTeachingType { get; init; }
}


