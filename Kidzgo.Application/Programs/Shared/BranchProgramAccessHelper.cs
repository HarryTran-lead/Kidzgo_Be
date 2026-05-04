using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.Shared;

internal static class BranchProgramAccessHelper
{
    public static Task<bool> IsProgramAssignedToBranchAsync(
        IDbContext context,
        Guid branchId,
        Guid programId,
        CancellationToken cancellationToken)
    {
        return context.BranchPrograms
            .AnyAsync(
                bp => bp.BranchId == branchId &&
                      bp.ProgramId == programId &&
                      bp.IsActive,
                cancellationToken);
    }

    public static Task<Guid?> GetDefaultMakeupClassIdAsync(
        IDbContext context,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        return context.BranchPrograms
            .Where(bp => bp.BranchId == branchId &&
                         bp.IsActive &&
                         bp.Program.IsMakeup &&
                         bp.DefaultMakeupClassId != null)
            .OrderByDescending(bp => bp.UpdatedAt)
            .Select(bp => bp.DefaultMakeupClassId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static IQueryable<Program> FilterProgramsByBranch(
        IQueryable<Program> programsQuery,
        Guid branchId)
    {
        return programsQuery.Where(
            p => p.BranchPrograms.Any(bp => bp.BranchId == branchId && bp.IsActive));
    }

}
