using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class CurriculumAssignmentAccessHelper
{
    public static Task<bool> IsSyllabusAssignedToBranchAsync(
        IDbContext context,
        Guid branchId,
        Guid programId,
        Guid levelId,
        Guid syllabusId,
        DateOnly effectiveDate,
        CancellationToken cancellationToken)
    {
        var effectiveUtc = effectiveDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return context.CurriculumAssignments
            .AnyAsync(
                x => x.BranchId == branchId &&
                     x.ProgramId == programId &&
                     x.LevelId == levelId &&
                     x.SyllabusId == syllabusId &&
                     x.IsActive &&
                     (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value <= effectiveUtc) &&
                     (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= effectiveUtc),
                cancellationToken);
    }

    public static IQueryable<CurriculumAssignment> QueryAssignmentsForBranch(
        IDbContext context,
        Guid branchId)
    {
        return context.CurriculumAssignments
            .Where(x => x.BranchId == branchId && x.IsActive);
    }
}
