using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.TuitionPlans.Shared;
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
            tuitionPlansQuery = tuitionPlansQuery.Where(t =>
                t.ModuleId == query.ModuleId.Value ||
                t.SelectedModules.Any(x => x.ModuleId == query.ModuleId.Value));
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
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Include(t => t.Module)
            .Include(t => t.SelectedModules)
                .ThenInclude(x => x.Module)
            .Include(t => t.LearningTicketType)
            .Include(t => t.CurriculumMappings)
                .ThenInclude(x => x.Syllabus)
            .OrderByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Name)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var items = tuitionPlans.Select(t =>
        {
            var syllabus = TuitionPlanSelectionSupport.ResolveActiveSyllabus(t);
            var modules = TuitionPlanSelectionSupport.ResolveModules(t);

            return new TuitionPlanDto
            {
                Id = t.Id,
                ProgramId = t.ProgramId,
                LevelId = t.LevelId,
                LevelName = t.Level.Name,
                SyllabusId = syllabus?.SyllabusId,
                SyllabusCode = syllabus?.SyllabusCode,
                SyllabusVersion = syllabus?.SyllabusVersion,
                SyllabusTitle = syllabus?.SyllabusTitle,
                ModuleId = TuitionPlanSelectionSupport.ResolvePrimaryModuleId(t),
                ModuleName = TuitionPlanSelectionSupport.ResolvePrimaryModuleName(t),
                ModuleIds = TuitionPlanSelectionSupport.ResolveModuleIds(t),
                Modules = modules,
                LearningTicketTypeId = t.LearningTicketTypeId,
                LearningTicketTypeCode = t.LearningTicketType?.Code,
                ProgramName = t.Program.Name,
                Name = t.Name,
                TotalSessions = t.TotalSessions,
                TuitionAmount = t.TuitionAmount,
                UnitPriceSession = t.UnitPriceSession,
                Currency = t.Currency,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };
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

