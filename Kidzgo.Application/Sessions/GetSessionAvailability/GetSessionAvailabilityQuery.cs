using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetSessionAvailability;

public sealed class GetSessionAvailabilityQuery : IQuery<GetSessionAvailabilityResponse>
{
    public DateTime ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ExcludeSessionId { get; init; }
    public bool IncludeUnavailable { get; init; }
}
