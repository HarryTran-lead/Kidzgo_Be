using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetBranchPrograms;

public sealed class GetBranchProgramsQueryHandler(IDbContext context)
    : IQueryHandler<GetBranchProgramsQuery, GetBranchProgramsResponse>
{
    public async Task<Result<GetBranchProgramsResponse>> Handle(
        GetBranchProgramsQuery query,
        CancellationToken cancellationToken)
    {
        var branchExists = await context.Branches
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.BranchId, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<GetBranchProgramsResponse>(BranchErrors.NotFound(query.BranchId));
        }

        var programs = await context.BranchPrograms
            .AsNoTracking()
            .Where(x => x.BranchId == query.BranchId && x.IsActive)
            .OrderBy(x => x.Program.Name)
            .Select(x => new BranchProgramDto
            {
                BranchProgramId = x.Id,
                ProgramId = x.ProgramId,
                ProgramName = x.Program.Name,
                ProgramCode = x.Program.Code,
                IsActive = x.IsActive,
                DefaultMakeupClassId = x.DefaultMakeupClassId
            })
            .ToListAsync(cancellationToken);

        return new GetBranchProgramsResponse
        {
            Programs = programs
        };
    }
}
