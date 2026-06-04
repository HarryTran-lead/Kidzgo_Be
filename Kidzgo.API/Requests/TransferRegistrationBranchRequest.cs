using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class TransferRegistrationBranchRequest
{
    public Guid NewBranchId { get; init; }
    public Guid? NewClassId { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public string? Reason { get; init; }
    public List<WeeklyPatternEntry>? WeeklyPattern { get; init; }
}
