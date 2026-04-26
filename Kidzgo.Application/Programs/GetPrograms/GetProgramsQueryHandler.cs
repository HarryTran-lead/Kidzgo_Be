using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetPrograms;

public sealed class GetProgramsQueryHandler(
    IDbContext context
) : IQueryHandler<GetProgramsQuery, GetProgramsResponse>
{
    public async Task<Result<GetProgramsResponse>> Handle(GetProgramsQuery query, CancellationToken cancellationToken)
    {
        var programsQuery = context.Programs
            .Where(p => !p.IsDeleted);

        if (query.BranchId.HasValue)
        {
            programsQuery = BranchProgramAccessHelper.FilterProgramsByBranch(programsQuery, query.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            programsQuery = programsQuery.Where(p =>
                p.Name.Contains(query.SearchTerm) ||
                (p.Code != null && p.Code.Contains(query.SearchTerm)) ||
                (p.Description != null && p.Description.Contains(query.SearchTerm)));
        }

        if (query.IsActive.HasValue)
        {
            programsQuery = programsQuery.Where(p => p.IsActive == query.IsActive.Value);
        }

        if (query.IsMakeup.HasValue)
        {
            programsQuery = programsQuery.Where(p => p.IsMakeup == query.IsMakeup.Value);
        }

        int totalCount = await programsQuery.CountAsync(cancellationToken);
        var branchId = query.BranchId;

        var programs = await programsQuery
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Name)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(p => new ProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                IsMakeup = p.IsMakeup,
                IsSupplementary = p.IsSupplementary,
                BaseFee = p.TuitionPlans
                    .Where(tp => tp.IsActive &&
                                 !tp.IsDeleted &&
                                 (!branchId.HasValue ||
                                  tp.BranchId == null ||
                                  tp.BranchId == branchId))
                    .Select(tp => (decimal?)tp.TuitionAmount)
                    .Min() ?? 0,
                Fee = p.TuitionPlans
                    .Where(tp => tp.IsActive &&
                                 !tp.IsDeleted &&
                                 (!branchId.HasValue ||
                                  tp.BranchId == null ||
                                  tp.BranchId == branchId))
                    .Select(tp => (decimal?)tp.TuitionAmount)
                    .Min() ?? 0,
                Description = p.Description,
                IsActive = p.IsActive,
                AssignedBranchCount = p.BranchPrograms.Count(bp => bp.IsActive),
                ClassCount = p.Classes.Count(c =>
                    c.Status != Domain.Classes.ClassStatus.Cancelled &&
                    (!branchId.HasValue || c.BranchId == branchId)),
                StudentCount = p.Classes
                    .Where(c => !branchId.HasValue || c.BranchId == branchId)
                    .SelectMany(c => c.ClassEnrollments)
                    .Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active)
            })
            .ToListAsync(cancellationToken);

        var page = new Page<ProgramDto>(
            programs,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetProgramsResponse
        {
            Programs = page
        };
    }
}
