namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid BranchId { get; init; }
    public Guid? ModuleId { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public int? SessionIndexInModule { get; init; }
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public string SectionType { get; init; } = null!;
    public Guid SessionId => Id;
    public DateOnly PlannedDate => VietnamTime.ToVietnamDateOnly(PlannedDatetime);
    public TimeOnly StartTime => VietnamTime.ToVietnamTimeOnly(PlannedDatetime);
    public TimeOnly EndTime => VietnamTime.ToVietnamTimeOnly(PlannedDatetime.AddMinutes(DurationMinutes));
}


