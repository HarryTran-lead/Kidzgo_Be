using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionRules;

public sealed class GetProgramProgressionRulesQueryHandler(
    IDbContext context) : IQueryHandler<GetProgramProgressionRulesQuery, GetProgramProgressionRulesResponse>
{
    public async Task<Result<GetProgramProgressionRulesResponse>> Handle(
        GetProgramProgressionRulesQuery query,
        CancellationToken cancellationToken)
    {
        var rulesQuery = context.ProgramProgressionRules
            .AsNoTracking()
            .Include(r => r.SourceLevel)
            .Include(r => r.TargetLevel)
            .Include(r => r.SourceProgram)
            .Include(r => r.TargetProgram)
            .AsQueryable();

        if (query.SourceLevelId.HasValue)
        {
            rulesQuery = rulesQuery.Where(r => r.SourceLevelId == query.SourceLevelId.Value);
        }

        if (query.TargetLevelId.HasValue)
        {
            rulesQuery = rulesQuery.Where(r => r.TargetLevelId == query.TargetLevelId.Value);
        }

        if (query.SourceProgramId.HasValue)
        {
            rulesQuery = rulesQuery.Where(r => r.SourceProgramId == query.SourceProgramId.Value);
        }

        if (query.IsActive.HasValue)
        {
            rulesQuery = rulesQuery.Where(r => r.IsActive == query.IsActive.Value);
        }

        var rules = await rulesQuery
            .OrderBy(r => r.SourceProgram.Name)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(new GetProgramProgressionRulesResponse
        {
            Rules = rules.Select(rule => rule.ToDto()).ToList(),
            TotalCount = rules.Count
        });
    }
}
