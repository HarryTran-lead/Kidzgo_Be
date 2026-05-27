namespace Kidzgo.API.Requests;

public sealed class UpsertBranchSyllabusRequest
{
    public Guid SyllabusId { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
