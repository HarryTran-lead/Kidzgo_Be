using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SlotTypes.GetSlotTypes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SlotTypes.GetSlotTypeById;

public sealed class GetSlotTypeByIdQueryHandler(
    IDbContext context)
    : IQueryHandler<GetSlotTypeByIdQuery, SlotTypeDto>
{
    public async Task<Result<SlotTypeDto>> Handle(
        GetSlotTypeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var item = await context.SlotTypes
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new SlotTypeDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                DayGroup = x.DayGroup,
                TimeBand = x.TimeBand,
                TeacherType = x.TeacherType,
                UsageType = x.UsageType,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return Result.Failure<SlotTypeDto>(
                Error.NotFound(
                    "SlotType.NotFound",
                    $"Slot type '{query.Id}' was not found."));
        }

        return Result.Success(item);
    }
}

