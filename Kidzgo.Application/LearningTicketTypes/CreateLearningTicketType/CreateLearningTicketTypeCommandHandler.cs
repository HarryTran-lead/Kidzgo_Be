using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTicketTypes.CreateLearningTicketType;

public sealed class CreateLearningTicketTypeCommandHandler(
    IDbContext context)
    : ICommandHandler<CreateLearningTicketTypeCommand, LearningTicketTypeDto>
{
    public async Task<Result<LearningTicketTypeDto>> Handle(
        CreateLearningTicketTypeCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedCode = command.Code.Trim().ToUpperInvariant();

        var exists = await context.LearningTicketTypes
            .AnyAsync(x => x.Code == normalizedCode, cancellationToken);

        if (exists)
        {
            return Result.Failure<LearningTicketTypeDto>(
                Error.Conflict(
                    "LearningTicketType.CodeExists",
                    $"Learning ticket type code '{normalizedCode}' already exists."));
        }

        var now = VietnamTime.UtcNow();
        var item = new LearningTicketType
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = command.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            CompatibilityMode = command.CompatibilityMode,
            AllowedDayGroups = TicketCompatibilityRuleSupport.CombineDayGroups(command.AllowedDayGroups),
            AllowedTimeBands = TicketCompatibilityRuleSupport.CombineTimeBands(command.AllowedTimeBands),
            AllowedTeacherTypes = TicketCompatibilityRuleSupport.CombineTeacherTypes(command.AllowedTeacherTypes),
            AllowedUsageTypes = TicketCompatibilityRuleSupport.CombineUsageTypes(command.AllowedUsageTypes),
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.LearningTicketTypes.Add(item);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LearningTicketTypeDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            CompatibilityMode = item.CompatibilityMode,
            AllowedDayGroups = TicketCompatibilityRuleSupport.ExpandDayGroups(item.AllowedDayGroups),
            AllowedTimeBands = TicketCompatibilityRuleSupport.ExpandTimeBands(item.AllowedTimeBands),
            AllowedTeacherTypes = TicketCompatibilityRuleSupport.ExpandTeacherTypes(item.AllowedTeacherTypes),
            AllowedUsageTypes = TicketCompatibilityRuleSupport.ExpandUsageTypes(item.AllowedUsageTypes),
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        });
    }
}

