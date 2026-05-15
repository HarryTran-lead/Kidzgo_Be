using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilityById;

public sealed class GetTicketTypeCompatibilityByIdQueryHandler(
    IDbContext context)
    : IQueryHandler<GetTicketTypeCompatibilityByIdQuery, TicketTypeCompatibilityDto>
{
    public async Task<Result<TicketTypeCompatibilityDto>> Handle(
        GetTicketTypeCompatibilityByIdQuery query,
        CancellationToken cancellationToken)
    {
        var item = await context.TicketTypeCompatibilities
            .AsNoTracking()
            .Include(x => x.LearningTicketType)
            .Include(x => x.SlotType)
            .Where(x => x.Id == query.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return Result.Failure<TicketTypeCompatibilityDto>(
                Error.NotFound(
                    "TicketTypeCompatibility.NotFound",
                    $"Ticket type compatibility '{query.Id}' was not found."));
        }

        return Result.Success(item);
    }
}

