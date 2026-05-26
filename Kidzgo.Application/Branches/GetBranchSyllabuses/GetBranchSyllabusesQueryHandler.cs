using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.GetBranchSyllabuses;

public sealed class GetBranchSyllabusesQueryHandler(IDbContext context)
    : IQueryHandler<GetBranchSyllabusesQuery, GetBranchSyllabusesResponse>
{
    public async Task<Result<GetBranchSyllabusesResponse>> Handle(
        GetBranchSyllabusesQuery query,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        var syllabuses = await context.CurriculumAssignments
            .AsNoTracking()
            .Where(x => x.BranchId == query.BranchId &&
                        x.IsActive &&
                        x.Syllabus.IsActive &&
                        !x.Syllabus.IsDeleted &&
                        (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value <= now) &&
                        (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= now))
            .OrderBy(x => x.Program.Name)
            .ThenBy(x => x.Level.Name)
            .ThenBy(x => x.Syllabus.Code)
            .ThenByDescending(x => x.Syllabus.Version)
            .Select(x => new BranchSyllabusDto
            {
                CurriculumAssignmentId = x.Id,
                SyllabusId = x.SyllabusId,
                ProgramId = x.ProgramId,
                ProgramName = x.Program.Name,
                LevelId = x.LevelId,
                LevelName = x.Level.Name,
                Code = x.Syllabus.Code,
                Version = x.Syllabus.Version,
                Title = x.Syllabus.Title,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                IsActive = true
            })
            .ToListAsync(cancellationToken);

        return new GetBranchSyllabusesResponse
        {
            Syllabuses = syllabuses
        };
    }
}
