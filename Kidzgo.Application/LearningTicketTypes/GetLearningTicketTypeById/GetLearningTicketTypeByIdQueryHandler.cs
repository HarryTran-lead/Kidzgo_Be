using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypeById;

public sealed class GetLearningTicketTypeByIdQueryHandler(
    IDbContext context)
    : IQueryHandler<GetLearningTicketTypeByIdQuery, LearningTicketTypeDto>
{
    public async Task<Result<LearningTicketTypeDto>> Handle(
        GetLearningTicketTypeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var item = await context.LearningTicketTypes
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new LearningTicketTypeDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return Result.Failure<LearningTicketTypeDto>(
                Error.NotFound(
                    "LearningTicketType.NotFound",
                    $"Learning ticket type '{query.Id}' was not found."));
        }

        return Result.Success(item);
    }
}

