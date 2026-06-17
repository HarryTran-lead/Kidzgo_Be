using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.ExpireMakeupCredit;

public sealed class ExpireMakeupCreditCommand : ICommand
{
    public Guid MakeupCreditId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public sealed class ExpireMakeupCreditCommandHandler(IDbContext context)
    : ICommandHandler<ExpireMakeupCreditCommand>
{
    public async Task<Result> Handle(ExpireMakeupCreditCommand command, CancellationToken cancellationToken)
    {
        var credit = await context.MakeupCredits
            .FirstOrDefaultAsync(c => c.Id == command.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.MakeupCreditId));
        }

        if (credit.Status != MakeupCreditStatus.Available)
        {
            return Result.Failure(MakeupCreditErrors.NotAvailable(command.MakeupCreditId));
        }

        credit.Status = MakeupCreditStatus.Expired;
        credit.ExpiresAt = command.ExpiresAt ?? VietnamTime.UtcNow();
        credit.UsedSessionId = null;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

