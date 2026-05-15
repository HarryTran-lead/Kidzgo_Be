using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.DeleteTicketTypeCompatibility;

public sealed class DeleteTicketTypeCompatibilityCommandHandler(
    IDbContext context)
    : ICommandHandler<DeleteTicketTypeCompatibilityCommand>
{
    public async Task<Result> Handle(DeleteTicketTypeCompatibilityCommand command, CancellationToken cancellationToken)
    {
        var item = await context.TicketTypeCompatibilities
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (item is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "TicketTypeCompatibility.NotFound",
                    $"Ticket type compatibility '{command.Id}' was not found."));
        }

        context.TicketTypeCompatibilities.Remove(item);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

