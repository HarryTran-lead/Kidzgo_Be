using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionSchedules;

public sealed class GetProgramProgressionSchedulesQuery : IQuery<GetProgramProgressionSchedulesResponse>
{
    public Guid? SourceClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? AssignedTeacherUserId { get; init; }
    public ProgramProgressionScheduleStatus? Status { get; init; }
    public ProgramProgressionScheduleParticipantStatus? ParticipantStatus { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
