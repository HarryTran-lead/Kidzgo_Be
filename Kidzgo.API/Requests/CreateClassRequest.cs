using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class CreateClassRequest
{
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid StartModuleId { get; set; }
    public int StartSessionIndex { get; set; } = 1;
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string? Name { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? MainTeacherId { get; set; }
    public Guid? AssistantTeacherId { get; set; }
    public Guid? SlotTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int Capacity { get; set; }
    public int? SessionsToGenerate { get; set; }
    public bool SkipHolidays { get; set; } = true;
    public ClassScheduleRequest? Schedule { get; set; }
    public List<ScheduleSlot>? WeeklyScheduleSlots { get; set; }
    public string? Description { get; set; }
}

public sealed class ClassScheduleRequest
{
    public List<int> DaysOfWeek { get; set; } = [];
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
}

