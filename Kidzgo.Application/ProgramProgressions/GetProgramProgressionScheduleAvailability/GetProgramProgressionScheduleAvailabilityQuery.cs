using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleAvailability;

public sealed class GetProgramProgressionScheduleAvailabilityQuery : IQuery<GetProgramProgressionScheduleAvailabilityResponse>
{
    public Guid SourceClassId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? ExcludeScheduleId { get; init; }
    public bool IncludeUnavailable { get; init; }
}
