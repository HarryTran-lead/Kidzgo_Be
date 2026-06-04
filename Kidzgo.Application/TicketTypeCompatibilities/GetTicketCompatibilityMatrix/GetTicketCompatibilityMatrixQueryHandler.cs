using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketCompatibilityMatrix;

public sealed class GetTicketCompatibilityMatrixQueryHandler(
    IDbContext context,
    TicketCompatibilityService ticketCompatibilityService)
    : IQueryHandler<GetTicketCompatibilityMatrixQuery, GetTicketCompatibilityMatrixResponse>
{
    public async Task<Result<GetTicketCompatibilityMatrixResponse>> Handle(
        GetTicketCompatibilityMatrixQuery query,
        CancellationToken cancellationToken)
    {
        var learningTicketTypesQuery = context.LearningTicketTypes
            .AsNoTracking()
            .AsQueryable();

        var slotTypesQuery = context.SlotTypes
            .AsNoTracking()
            .AsQueryable();

        if (query.LearningTicketTypeId.HasValue)
        {
            learningTicketTypesQuery = learningTicketTypesQuery
                .Where(x => x.Id == query.LearningTicketTypeId.Value);
        }

        if (query.OnlyActive)
        {
            learningTicketTypesQuery = learningTicketTypesQuery.Where(x => x.IsActive);
            slotTypesQuery = slotTypesQuery.Where(x => x.IsActive);
        }

        var learningTicketTypes = await learningTicketTypesQuery
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var slotTypes = await slotTypesQuery
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var slotTypeIds = slotTypes
            .Select(x => x.Id)
            .ToList();

        var cells = new List<TicketCompatibilityMatrixCellDto>();
        foreach (var learningTicketType in learningTicketTypes)
        {
            var evaluations = await ticketCompatibilityService.EvaluateForSlotTypesAsync(
                learningTicketType.Id,
                slotTypeIds,
                cancellationToken);

            foreach (var slotType in slotTypes)
            {
                if (!evaluations.TryGetValue(slotType.Id, out var evaluation))
                {
                    continue;
                }

                cells.Add(new TicketCompatibilityMatrixCellDto
                {
                    LearningTicketTypeId = learningTicketType.Id,
                    SlotTypeId = slotType.Id,
                    IsCompatible = evaluation.IsCompatible,
                    OverrideValue = evaluation.OverrideValue,
                    Source = evaluation.Source,
                    Reason = evaluation.Reason
                });
            }
        }

        return Result.Success(new GetTicketCompatibilityMatrixResponse
        {
            LearningTicketTypes = learningTicketTypes
                .Select(x => new TicketCompatibilityMatrixLearningTicketTypeDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompatibilityMode = x.CompatibilityMode,
                    IsActive = x.IsActive
                })
                .ToList(),
            SlotTypes = slotTypes
                .Select(x => new TicketCompatibilityMatrixSlotTypeDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    DayGroup = x.DayGroup,
                    TimeBand = x.TimeBand,
                    TeacherType = x.TeacherType,
                    UsageType = x.UsageType,
                    IsActive = x.IsActive
                })
                .ToList(),
            Cells = cells
        });
    }
}
