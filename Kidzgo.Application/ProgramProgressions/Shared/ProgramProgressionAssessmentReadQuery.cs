using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.Shared;

internal static class ProgramProgressionAssessmentReadQuery
{
    public static IQueryable<ProgramProgressionAssessment> Build(IDbContext context)
    {
        return context.ProgramProgressionAssessments
            .AsNoTracking()
            .Include(a => a.Rule)
            .Include(a => a.ScheduleParticipant)
                .ThenInclude(participant => participant!.Schedule)
                    .ThenInclude(schedule => schedule.SourceClass)
            .Include(a => a.StudentProfile)
            .Include(a => a.SourceProgram)
            .Include(a => a.TargetProgram)
            .Include(a => a.SourceEnrollment)
                .ThenInclude(enrollment => enrollment!.Class)
            .Include(a => a.ApprovedTuitionPlan);
    }
}
