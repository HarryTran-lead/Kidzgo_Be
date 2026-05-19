using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CompleteSession;

public sealed class CompleteSessionCommandHandler(
    IDbContext context,
    ClassProgressionService classProgressionService
) : ICommandHandler<CompleteSessionCommand>
{
    public async Task<Result> Handle(CompleteSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status == SessionStatus.Cancelled)
        {
            return Result.Failure(SessionErrors.Cancelled);
        }

        if (session.Status == SessionStatus.Completed)
        {
            return Result.Failure(SessionErrors.InvalidStatus);
        }

        var actualUtc = command.ActualDatetime switch
        {
            null => VietnamTime.UtcNow(),
            var dt => VietnamTime.NormalizeToUtc(dt)
        };

        session.Status = SessionStatus.Completed;
        session.ActualDatetime = actualUtc;
        session.UpdatedAt = VietnamTime.UtcNow();

        await classProgressionService.AdvanceAsync(session.ClassId, session.ModuleId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


