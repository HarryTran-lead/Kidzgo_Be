using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

public sealed class GetTicketTypeCompatibilitiesQueryHandler(
    IDbContext context)
    : IQueryHandler<GetTicketTypeCompatibilitiesQuery, GetTicketTypeCompatibilitiesResponse>
{
    public async Task<Result<GetTicketTypeCompatibilitiesResponse>> Handle(
        GetTicketTypeCompatibilitiesQuery query,
        CancellationToken cancellationToken)
    {
        var source = context.TicketTypeCompatibilities
            .AsNoTracking()
            .Include(x => x.LearningTicketType)
            .Include(x => x.SlotType)
            .AsQueryable();

        if (query.LearningTicketTypeId.HasValue)
        {
            source = source.Where(x => x.LearningTicketTypeId == query.LearningTicketTypeId.Value);
        }

        if (query.SlotTypeId.HasValue)
        {
            source = source.Where(x => x.SlotTypeId == query.SlotTypeId.Value);
        }

        var items = await source
            .OrderBy(x => x.LearningTicketType.Code)
            .ThenBy(x => x.SlotType.Code)
            .Select(x => new TicketTypeCompatibilityDto
            {
                Id = x.Id,
                LearningTicketTypeId = x.LearningTicketTypeId,
                LearningTicketTypeCode = x.LearningTicketType.Code,
                SlotTypeId = x.SlotTypeId,
                SlotTypeCode = x.SlotType.Code,
                IsCompatible = x.IsCompatible,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetTicketTypeCompatibilitiesResponse
        {
            Items = items
        });
    }
}

