using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetSyllabusVersions;

public sealed class GetSyllabusVersionsQueryHandler(IDbContext context)
    : IQueryHandler<GetSyllabusVersionsQuery, GetSyllabusVersionsResponse>
{
    public async Task<Result<GetSyllabusVersionsResponse>> Handle(
        GetSyllabusVersionsQuery query,
        CancellationToken cancellationToken)
    {
        var syllabuses = context.Syllabuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (query.ActiveOnly)
        {
            syllabuses = syllabuses.Where(x => x.IsActive);
        }

        if (query.ProgramId.HasValue)
        {
            syllabuses = syllabuses.Where(x => x.ProgramId == query.ProgramId.Value);
        }

        if (query.LevelId.HasValue)
        {
            syllabuses = syllabuses.Where(x => x.LevelId == query.LevelId.Value);
        }

        if (query.BranchId.HasValue)
        {
            var syllabusIds = context.CurriculumAssignments
                .Where(x => x.BranchId == query.BranchId.Value && x.IsActive)
                .Select(x => x.SyllabusId);
            syllabuses = syllabuses.Where(x => syllabusIds.Contains(x.Id));
        }

        var versions = await syllabuses
            .OrderBy(x => x.Program.Name)
            .ThenBy(x => x.Level.Name)
            .ThenBy(x => x.Code)
            .ThenByDescending(x => x.Version)
            .Select(x => new SyllabusVersionDto
            {
                SyllabusId = x.Id,
                ProgramId = x.ProgramId,
                ProgramName = x.Program.Name,
                LevelId = x.LevelId,
                LevelName = x.Level.Name,
                Code = x.Code,
                Version = x.Version,
                Title = x.Title,
                Edition = x.Edition,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new GetSyllabusVersionsResponse
        {
            Versions = versions
        };
    }
}
