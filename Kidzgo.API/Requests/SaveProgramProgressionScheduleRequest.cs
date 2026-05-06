namespace Kidzgo.API.Requests;

public sealed class SaveProgramProgressionScheduleRequest
{
    public Guid SourceClassId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? AssignedTeacherUserId { get; set; }
    public string? Notes { get; set; }
    public List<Guid>? StudentProfileIds { get; set; }
}
