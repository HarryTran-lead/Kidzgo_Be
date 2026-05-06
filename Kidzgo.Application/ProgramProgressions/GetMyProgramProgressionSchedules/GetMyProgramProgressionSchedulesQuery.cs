using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.GetMyProgramProgressionSchedules;

public sealed class GetMyProgramProgressionSchedulesQuery : IQuery<GetMyProgramProgressionSchedulesResponse>
{
    public Guid? StudentProfileId { get; init; }
    public ProgramProgressionScheduleStatus? Status { get; init; }
    public ProgramProgressionScheduleParticipantStatus? ParticipantStatus { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
