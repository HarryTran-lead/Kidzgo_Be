using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.ProgramProgressions.GetMyProgramProgressionSchedules;

public sealed class GetMyProgramProgressionSchedulesResponse
{
    public Page<ProgramProgressionScheduleDto> Schedules { get; init; } = new([], 0);
}
