using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;

public sealed class GetLearningTicketTypesQueryHandler(
    IDbContext context)
    : IQueryHandler<GetLearningTicketTypesQuery, GetLearningTicketTypesResponse>
{
    public async Task<Result<GetLearningTicketTypesResponse>> Handle(
        GetLearningTicketTypesQuery query,
        CancellationToken cancellationToken)
    {
        var source = context.LearningTicketTypes.AsNoTracking().AsQueryable();

        if (query.IsActive.HasValue)
        {
            source = source.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            source = source.Where(x =>
                x.Code.Contains(query.SearchTerm) ||
                x.Name.Contains(query.SearchTerm));
        }

        var items = await source
            .OrderBy(x => x.Code)
            .Select(x => new LearningTicketTypeDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompatibilityMode = x.CompatibilityMode,
                AllowedDayGroups = TicketCompatibilityRuleSupport.ExpandDayGroups(x.AllowedDayGroups),
                AllowedTimeBands = TicketCompatibilityRuleSupport.ExpandTimeBands(x.AllowedTimeBands),
                AllowedTeacherTypes = TicketCompatibilityRuleSupport.ExpandTeacherTypes(x.AllowedTeacherTypes),
                AllowedUsageTypes = TicketCompatibilityRuleSupport.ExpandUsageTypes(x.AllowedUsageTypes),
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLearningTicketTypesResponse
        {
            Items = items
        });
    }
}

