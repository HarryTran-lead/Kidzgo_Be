namespace Kidzgo.API.Requests;

public sealed class UpsertTicketTypeCompatibilityOverridesRequest
{
    public List<UpsertTicketTypeCompatibilityOverrideItemRequest> Items { get; set; } = new();
}

public sealed class UpsertTicketTypeCompatibilityOverrideItemRequest
{
    public Guid SlotTypeId { get; set; }
    public bool? IsCompatible { get; set; }
}
