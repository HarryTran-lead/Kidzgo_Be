namespace Kidzgo.API.Requests;

public sealed class UpdateTicketTypeCompatibilityRequest
{
    public Guid LearningTicketTypeId { get; set; }
    public Guid SlotTypeId { get; set; }
    public bool IsCompatible { get; set; }
}

