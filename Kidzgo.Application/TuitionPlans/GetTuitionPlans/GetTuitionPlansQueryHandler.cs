using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.GetTuitionPlans;

public sealed class GetTuitionPlansQueryHandler(
    IDbContext context
) : IQueryHandler<GetTuitionPlansQuery, GetTuitionPlansResponse>
{
    public async Task<Result<GetTuitionPlansResponse>> Handle(
        GetTuitionPlansQuery query,
        CancellationToken cancellationToken)
    {
        var tuitionPlansQuery = context.TuitionPlans
            .Where(t => !t.IsDeleted);

        if (query.BranchId.HasValue)
        {
            var branchId = query.BranchId.Value;
            tuitionPlansQuery = tuitionPlansQuery
                .Where(t => t.Program.BranchPrograms.Any(bp => bp.BranchId == branchId && bp.IsActive));
        }

        if (query.ProgramId.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.ProgramId == query.ProgramId.Value);
        }

        if (query.LevelId.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.LevelId == query.LevelId.Value);
        }

        if (query.IsActive.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.IsActive == query.IsActive.Value);
        }

        var totalCount = await tuitionPlansQuery.CountAsync(cancellationToken);

        var tuitionPlans = await tuitionPlansQuery
            .Include(t => t.Program)
            .Include(t => t.Level)
            .OrderByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Name)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var items = tuitionPlans.Select(t => new TuitionPlanDto
        {
            Id = t.Id,
            ProgramId = t.ProgramId,
            LevelId = t.LevelId,
            LevelName = t.Level.Name,
            ProgramName = t.Program.Name,
            Name = t.Name,
            TotalSessions = t.TotalSessions,
            TuitionAmount = t.TuitionAmount,
            UnitPriceSession = t.UnitPriceSession,
            Currency = t.Currency,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();

        var page = new Page<TuitionPlanDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetTuitionPlansResponse
        {
            TuitionPlans = page
        };
    }
}
