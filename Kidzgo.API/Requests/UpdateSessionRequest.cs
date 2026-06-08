namespace Kidzgo.API.Requests;

public sealed class UpdateSessionRequest
{
    public DateTime PlannedDatetime { get; set; }
    public int DurationMinutes { get; set; }
    public Guid? PlannedRoomId { get; set; }
    public Guid? PlannedTeacherId { get; set; }
    public Guid? PlannedAssistantId { get; set; }
    public Guid? SlotTypeId { get; set; }
    public string ParticipationType { get; set; } = "Main";
    public string? SectionType { get; set; }
}


