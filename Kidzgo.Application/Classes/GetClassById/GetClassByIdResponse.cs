using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.GetClassById;

public sealed class GetClassByIdResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid? SlotTypeId { get; init; }
    public string? SlotTypeCode { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? MainTeacherId { get; init; }
    public string? MainTeacherName { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public string? AssistantTeacherName { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Status { get; init; } = null!;
    public int Capacity { get; init; }
    public int CurrentEnrollmentCount { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Name => Title;
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public string? Description { get; init; }
    public List<Guid> TeacherIds { get; init; } = new();
    public List<string> TeacherNames { get; init; } = new();
    public string? ScheduleText => SchedulePatternSupport.BuildDisplayText(WeeklyScheduleSlots);
    public int StudentCount => CurrentEnrollmentCount;
    public int TotalSessions { get; init; }
    public int CompletedSessions { get; init; }
    public List<ClassScheduleSegmentDto> ScheduleSegments { get; init; } = new();
    public decimal ProgressPercent => TotalSessions <= 0
        ? 0
        : Math.Round((decimal)CompletedSessions * 100 / TotalSessions, 2);
}

public sealed class ClassScheduleSegmentDto
{
    public Guid Id { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
}

