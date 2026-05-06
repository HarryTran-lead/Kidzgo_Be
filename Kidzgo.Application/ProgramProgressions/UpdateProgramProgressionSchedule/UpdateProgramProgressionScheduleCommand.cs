using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionSchedule;

public sealed class UpdateProgramProgressionScheduleCommand : ICommand<ProgramProgressionScheduleDto>
{
    public Guid Id { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? RoomId { get; init; }
    public Guid? AssignedTeacherUserId { get; init; }
    public string? Notes { get; init; }
}
