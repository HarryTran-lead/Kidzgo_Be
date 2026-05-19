using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetSyllabuses;

public sealed class GetSyllabusesQueryHandler(IDbContext context)
    : IQueryHandler<GetSyllabusesQuery, GetSyllabusesResponse>
{
    public async Task<Result<GetSyllabusesResponse>> Handle(GetSyllabusesQuery query, CancellationToken cancellationToken)
    {
        var syllabusesQuery = context.Syllabuses.AsQueryable();

        if (!query.IncludeDeleted)
        {
            syllabusesQuery = syllabusesQuery.Where(x => !x.IsDeleted);
        }

        if (query.ProgramId.HasValue)
        {
            syllabusesQuery = syllabusesQuery.Where(x => x.ProgramId == query.ProgramId.Value);
        }

        if (query.LevelId.HasValue)
        {
            syllabusesQuery = syllabusesQuery.Where(x => x.LevelId == query.LevelId.Value);
        }

        if (query.IsActive.HasValue)
        {
            syllabusesQuery = syllabusesQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            syllabusesQuery = syllabusesQuery.Where(x =>
                x.Title.Contains(query.SearchTerm) ||
                x.Code.Contains(query.SearchTerm) ||
                x.Version.Contains(query.SearchTerm));
        }

        var totalCount = await syllabusesQuery.CountAsync(cancellationToken);

        var items = await syllabusesQuery
            .OrderByDescending(x => x.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(x => new SyllabusListItemDto
            {
                Id = x.Id,
                ProgramId = x.ProgramId,
                ProgramName = x.Program.Name,
                LevelId = x.LevelId,
                LevelName = x.Level.Name,
                Code = x.Code,
                Version = x.Version,
                Title = x.Title,
                IsActive = x.IsActive,
                UnitCount = x.Units.Count,
                SessionTemplateCount = x.SessionTemplates.Count,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetSyllabusesResponse
        {
            Syllabuses = new Page<SyllabusListItemDto>(items, totalCount, query.PageNumber, query.PageSize)
        };
    }
}
