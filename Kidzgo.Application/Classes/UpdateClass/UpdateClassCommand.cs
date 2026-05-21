using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassCommand : ICommand<UpdateClassResponse>
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid StartModuleId { get; init; }
    public int StartSessionIndex { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? RoomId { get; init; }
    public Guid? MainTeacherId { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public Guid? SlotTypeId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int Capacity { get; init; }
    public List<ScheduleSlot>? WeeklyScheduleSlots { get; init; }
    public string? Description { get; init; }
}

