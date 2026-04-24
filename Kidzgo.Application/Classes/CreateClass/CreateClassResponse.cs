using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? RoomId { get; init; }
    public Guid? MainTeacherId { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Status { get; init; } = null!;
    public int Capacity { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public string? Description { get; init; }
    public string Name => Title;
    public string? ScheduleText => SchedulePatternSupport.BuildDisplayText(WeeklyScheduleSlots);
}

