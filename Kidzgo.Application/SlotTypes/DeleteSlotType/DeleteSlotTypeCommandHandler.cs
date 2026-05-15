using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SlotTypes.DeleteSlotType;

public sealed class DeleteSlotTypeCommandHandler(
    IDbContext context)
    : ICommandHandler<DeleteSlotTypeCommand>
{
    public async Task<Result> Handle(DeleteSlotTypeCommand command, CancellationToken cancellationToken)
    {
        var item = await context.SlotTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "SlotType.NotFound",
                    $"Slot type '{command.Id}' was not found."));
        }

        var isReferenced = await context.Classes.AnyAsync(x => x.SlotTypeId == command.Id, cancellationToken) ||
                           await context.Sessions.AnyAsync(x => x.SlotTypeId == command.Id, cancellationToken) ||
                           await context.TicketTypeCompatibilities.AnyAsync(x => x.SlotTypeId == command.Id, cancellationToken);

        if (isReferenced)
        {
            return Result.Failure(
                Error.Conflict(
                    "SlotType.InUse",
                    "Cannot delete slot type because it is being used."));
        }

        context.SlotTypes.Remove(item);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

