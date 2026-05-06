using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionSchedule;

public sealed class CreateProgramProgressionScheduleCommand : ICommand<ProgramProgressionScheduleDto>
{
    public Guid SourceClassId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? RoomId { get; init; }
    public Guid? AssignedTeacherUserId { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyCollection<Guid>? StudentProfileIds { get; init; }
}
