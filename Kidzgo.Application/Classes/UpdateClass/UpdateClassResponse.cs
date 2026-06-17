using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid? SyllabusId { get; init; }
    public string? SyllabusCode { get; init; }
    public int? SyllabusVersion { get; init; }
    public string? SyllabusTitle { get; init; }
    public Guid StartModuleId { get; init; }
    public int StartSessionIndex { get; init; }
    public Guid CurrentModuleId { get; init; }
    public int CurrentSessionIndex { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? RoomId { get; init; }
    public Guid? MainTeacherId { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? ExpectedEndDate { get; init; }
    public DateOnly? ActualEndDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Status { get; init; } = null!;
    public int Capacity { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public string? Description { get; init; }
    public string Name => Title;
    public string? ScheduleText => SchedulePatternSupport.BuildDisplayText(WeeklyScheduleSlots);
}

