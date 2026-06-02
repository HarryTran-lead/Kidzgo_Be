using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Students.Shared;

internal static class StudentBranchReadModelBuilder
{
    public static async Task<Result<StudentBranchStateDto>> BuildAsync(
        IDbContext context,
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var state = await context.StudentBranchStates
            .AsNoTracking()
            .Include(x => x.HomeBranch)
            .Include(x => x.ActiveBranch)
            .FirstOrDefaultAsync(x => x.StudentProfileId == studentProfileId, cancellationToken);

        if (state is null)
        {
            return Result.Failure<StudentBranchStateDto>(StudentBranchErrors.StudentNotFound(studentProfileId));
        }

        var transfers = await context.StudentBranchTransfers
            .AsNoTracking()
            .Where(x => x.StudentProfileId == studentProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new StudentBranchTransferDto
            {
                Id = x.Id,
                FromBranchId = x.FromBranchId,
                FromBranchName = x.FromBranch.Name,
                ToBranchId = x.ToBranchId,
                ToBranchName = x.ToBranch.Name,
                EffectiveDate = x.EffectiveDate,
                Reason = x.Reason,
                KeepCurrentClass = x.KeepCurrentClass,
                AllowCrossBranchEnrollment = x.AllowCrossBranchEnrollment,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new StudentBranchStateDto
        {
            StudentProfileId = studentProfileId,
            HomeBranchId = state.HomeBranchId,
            HomeBranchName = state.HomeBranch.Name,
            ActiveBranchId = state.ActiveBranchId,
            ActiveBranchName = state.ActiveBranch.Name,
            AllowCrossBranchEnrollment = state.AllowCrossBranchEnrollment,
            LastTransferredAt = state.LastTransferredAt,
            Transfers = transfers
        });
    }
}
