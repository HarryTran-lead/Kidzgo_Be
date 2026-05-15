using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SlotTypes.GetSlotTypes;

public sealed class GetSlotTypesQueryHandler(
    IDbContext context)
    : IQueryHandler<GetSlotTypesQuery, GetSlotTypesResponse>
{
    public async Task<Result<GetSlotTypesResponse>> Handle(
        GetSlotTypesQuery query,
        CancellationToken cancellationToken)
    {
        var source = context.SlotTypes.AsNoTracking().AsQueryable();

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
            .Select(x => new SlotTypeDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetSlotTypesResponse
        {
            Items = items
        });
    }
}

