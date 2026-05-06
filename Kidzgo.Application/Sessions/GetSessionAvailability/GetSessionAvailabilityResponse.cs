namespace Kidzgo.Application.Sessions.GetSessionAvailability;

public sealed class GetSessionAvailabilityResponse
{
    public DateTime ScheduledAt { get; init; }
    public DateTime EndAt { get; init; }
    public int DurationMinutes { get; init; }
    public List<SessionAvailableTeacherDto> Teachers { get; init; } = new();
    public List<SessionAvailableRoomDto> Rooms { get; init; } = new();
}

public sealed class SessionAvailableTeacherDto
{
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public string Role { get; init; } = null!;
    public Guid? BranchId { get; init; }
    public bool IsAvailable { get; init; }
    public List<SessionScheduleConflict> Conflicts { get; init; } = new();
}

public sealed class SessionAvailableRoomDto
{
    public Guid RoomId { get; init; }
    public string Name { get; init; } = null!;
    public Guid BranchId { get; init; }
    public int Capacity { get; init; }
    public bool IsAvailable { get; init; }
    public List<SessionScheduleConflict> Conflicts { get; init; } = new();
}

public sealed class SessionScheduleConflict
{
    public string Type { get; init; } = null!;
    public Guid ReferenceId { get; init; }
    public string Title { get; init; } = null!;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
}
