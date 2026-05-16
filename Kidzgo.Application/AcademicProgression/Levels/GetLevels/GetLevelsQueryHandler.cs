using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Levels.GetLevels;

public sealed class GetLevelsQueryHandler(IDbContext context)
    : IQueryHandler<GetLevelsQuery, GetLevelsResponse>
{
    public async Task<Result<GetLevelsResponse>> Handle(GetLevelsQuery query, CancellationToken cancellationToken)
    {
        var source = context.Levels.AsNoTracking().AsQueryable();

        if (query.ProgramId.HasValue)
        {
            source = source.Where(x => x.ProgramId == query.ProgramId.Value);
        }

        if (query.IsActive.HasValue)
        {
            source = source.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            source = source.Where(x => x.Code.Contains(query.SearchTerm) || x.Name.Contains(query.SearchTerm));
        }

        var items = await source
            .OrderBy(x => x.ProgramId)
            .ThenBy(x => x.Order)
            .Select(x => new LevelDto
            {
                Id = x.Id,
                ProgramId = x.ProgramId,
                Code = x.Code,
                Name = x.Name,
                Order = x.Order,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLevelsResponse { Items = items });
    }
}
