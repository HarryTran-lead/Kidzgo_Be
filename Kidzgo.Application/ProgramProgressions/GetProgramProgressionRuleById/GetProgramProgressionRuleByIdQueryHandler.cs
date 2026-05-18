using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionRuleById;

public sealed class GetProgramProgressionRuleByIdQueryHandler(
    IDbContext context) : IQueryHandler<GetProgramProgressionRuleByIdQuery, ProgramProgressionRuleDto>
{
    public async Task<Result<ProgramProgressionRuleDto>> Handle(
        GetProgramProgressionRuleByIdQuery query,
        CancellationToken cancellationToken)
    {
        var rule = await context.ProgramProgressionRules
            .AsNoTracking()
            .Include(r => r.SourceLevel)
            .Include(r => r.TargetLevel)
            .Include(r => r.SourceProgram)
            .Include(r => r.TargetProgram)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<ProgramProgressionRuleDto>(
                ProgramProgressionErrors.RuleNotFound(query.Id));
        }

        return Result.Success(rule.ToDto());
    }
}
