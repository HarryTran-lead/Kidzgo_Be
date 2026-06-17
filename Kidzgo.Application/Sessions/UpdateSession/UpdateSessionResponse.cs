namespace Kidzgo.Application.Sessions.UpdateSession;

public sealed class UpdateSessionResponse
{
    public Guid Id { get; init; }
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public string SectionType { get; init; } = null!;
    public Guid SessionId => Id;
    public DateOnly PlannedDate => VietnamTime.ToVietnamDateOnly(PlannedDatetime);
    public TimeOnly StartTime => VietnamTime.ToVietnamTimeOnly(PlannedDatetime);
    public TimeOnly EndTime => VietnamTime.ToVietnamTimeOnly(PlannedDatetime.AddMinutes(DurationMinutes));
}


