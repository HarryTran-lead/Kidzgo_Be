namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleAvailability;

public sealed class GetProgramProgressionScheduleAvailabilityResponse
{
    public Guid SourceClassId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public DateTime EndAt { get; init; }
    public int DurationMinutes { get; init; }
    public List<AvailableProgressionTeacherDto> Teachers { get; init; } = new();
    public List<AvailableProgressionRoomDto> Rooms { get; init; } = new();
}

public sealed class AvailableProgressionTeacherDto
{
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public Guid? BranchId { get; init; }
    public string RoleInClass { get; init; } = null!;
    public bool IsAvailable { get; init; }
    public List<ProgramProgressionScheduleConflictDto> Conflicts { get; init; } = new();
}

public sealed class AvailableProgressionRoomDto
{
    public Guid RoomId { get; init; }
    public string Name { get; init; } = null!;
    public Guid BranchId { get; init; }
    public int Capacity { get; init; }
    public bool IsAvailable { get; init; }
    public List<ProgramProgressionScheduleConflictDto> Conflicts { get; init; } = new();
}

public sealed class ProgramProgressionScheduleConflictDto
{
    public string Type { get; init; } = null!;
    public Guid ReferenceId { get; init; }
    public string Title { get; init; } = null!;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
}
