using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.Shared;

internal static class ProgramProgressionScheduleReadQuery
{
    public static IQueryable<ProgramProgressionSchedule> Build(IDbContext context)
    {
        return context.ProgramProgressionSchedules
            .AsNoTracking()
            .Include(schedule => schedule.SourceClass)
            .Include(schedule => schedule.SourceProgram)
            .Include(schedule => schedule.Branch)
            .Include(schedule => schedule.Room)
            .Include(schedule => schedule.AssignedTeacherUser)
            .Include(schedule => schedule.CreatedByUser)
            .Include(schedule => schedule.Participants)
                .ThenInclude(participant => participant.StudentProfile)
            .Include(schedule => schedule.Participants)
                .ThenInclude(participant => participant.Assessment);
    }
}
