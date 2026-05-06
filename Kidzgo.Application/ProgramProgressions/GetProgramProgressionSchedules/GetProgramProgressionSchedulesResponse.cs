using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionSchedules;

public sealed class GetProgramProgressionSchedulesResponse
{
    public Page<ProgramProgressionScheduleDto> Schedules { get; init; } = new([], 0);
}
