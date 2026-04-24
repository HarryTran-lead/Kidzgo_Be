using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class TransferClassRequest
{
    public Guid NewClassId { get; set; }
    public string Track { get; set; } = "primary";
    public List<WeeklyPatternEntry>? WeeklyPattern { get; set; }
    public DateTime? EffectiveDate { get; set; }
}
