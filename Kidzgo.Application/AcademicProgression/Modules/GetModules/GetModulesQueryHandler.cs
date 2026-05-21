using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Modules.GetModules;

public sealed class GetModulesQueryHandler(IDbContext context)
    : IQueryHandler<GetModulesQuery, GetModulesResponse>
{
    public async Task<Result<GetModulesResponse>> Handle(GetModulesQuery query, CancellationToken cancellationToken)
    {
        var source = context.Modules
            .AsNoTracking()
            .Include(x => x.Level)
            .AsQueryable();

        if (query.LevelId.HasValue)
        {
            source = source.Where(x => x.LevelId == query.LevelId.Value);
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
            .OrderBy(x => x.Level.Order)
            .ThenBy(x => x.Order)
            .Select(x => new ModuleDto
            {
                Id = x.Id,
                LevelId = x.LevelId,
                LevelCode = x.Level.Code,
                Code = x.Code,
                Name = x.Name,
                Order = x.Order,
                Description = x.Description,
                PlannedSessionCount = x.PlannedSessionCount,
                LessonPlanCount = x.LessonPlanTemplates.Count(t => t.IsActive && !t.IsDeleted),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetModulesResponse { Items = items });
    }
}
