using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.GetTuitionPlans;

public sealed class GetTuitionPlansQueryHandler(
    IDbContext context
) : IQueryHandler<GetTuitionPlansQuery, GetTuitionPlansResponse>
{
    public async Task<Result<GetTuitionPlansResponse>> Handle(GetTuitionPlansQuery query, CancellationToken cancellationToken)
    {
        var tuitionPlansQuery = context.TuitionPlans
            .Include(t => t.Program)
            .Where(t => !t.IsDeleted);

        if (query.BranchId.HasValue)
        {
            var branchId = query.BranchId.Value;
            tuitionPlansQuery = tuitionPlansQuery
                .Where(t => t.Program.BranchPrograms.Any(bp => bp.BranchId == branchId && bp.IsActive));
        }

        // Filter by program
        if (query.ProgramId.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.ProgramId == query.ProgramId.Value);
        }

        if (query.LevelId.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.LevelId == query.LevelId.Value);
        }

        if (query.ModuleId.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.ModuleId == query.ModuleId.Value);
        }

        // Filter by IsActive
        if (query.IsActive.HasValue)
        {
            tuitionPlansQuery = tuitionPlansQuery.Where(t => t.IsActive == query.IsActive.Value);
        }

        // Get total count
        int totalCount = await tuitionPlansQuery.CountAsync(cancellationToken);

        // Apply pagination
        var tuitionPlans = await tuitionPlansQuery
            .OrderByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Name)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(t => new TuitionPlanDto
            {
                Id = t.Id,
                ProgramId = t.ProgramId,
                LevelId = t.LevelId,
                LevelName = t.Level.Name,
                ModuleId = t.ModuleId,
                ModuleName = t.Module != null ? t.Module.Name : null,
                LearningTicketTypeId = t.LearningTicketTypeId,
                LearningTicketTypeCode = t.LearningTicketType != null ? t.LearningTicketType.Code : null,
                ProgramName = t.Program.Name,
                Name = t.Name,
                TotalSessions = t.TotalSessions,
                TuitionAmount = t.TuitionAmount,
                UnitPriceSession = t.UnitPriceSession,
                Currency = t.Currency,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<TuitionPlanDto>(
            tuitionPlans,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetTuitionPlansResponse
        {
            TuitionPlans = page
        };
    }
}

