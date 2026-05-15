using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTicketTypes.DeleteLearningTicketType;

public sealed class DeleteLearningTicketTypeCommandHandler(
    IDbContext context)
    : ICommandHandler<DeleteLearningTicketTypeCommand>
{
    public async Task<Result> Handle(DeleteLearningTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var item = await context.LearningTicketTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "LearningTicketType.NotFound",
                    $"Learning ticket type '{command.Id}' was not found."));
        }

        var isReferenced = await context.TuitionPlans.AnyAsync(x => x.LearningTicketTypeId == command.Id, cancellationToken) ||
                           await context.LearningTicketItems.AnyAsync(x => x.LearningTicketTypeId == command.Id, cancellationToken) ||
                           await context.TicketTypeCompatibilities.AnyAsync(x => x.LearningTicketTypeId == command.Id, cancellationToken);

        if (isReferenced)
        {
            return Result.Failure(
                Error.Conflict(
                    "LearningTicketType.InUse",
                    "Cannot delete learning ticket type because it is being used."));
        }

        context.LearningTicketTypes.Remove(item);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

