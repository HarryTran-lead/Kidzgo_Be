using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class CreateClassRequest
{
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string? Name { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? MainTeacherId { get; set; }
    public Guid? AssistantTeacherId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int Capacity { get; set; }
    public List<ScheduleSlot>? WeeklyScheduleSlots { get; set; }
    public string? Description { get; set; }
}

