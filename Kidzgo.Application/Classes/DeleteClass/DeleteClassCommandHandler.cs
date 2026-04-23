using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.DeleteClass;

public sealed class DeleteClassCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteClassCommand>
{
    public async Task<Result> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure(
                ClassErrors.NotFound(command.Id));
        }

        bool hasActiveEnrollments = await context.ClassEnrollments
            .AnyAsync(
                ce => ce.ClassId == command.Id &&
                      (ce.Status == EnrollmentStatus.Active || ce.Status == EnrollmentStatus.Paused),
                cancellationToken);

        if (hasActiveEnrollments)
        {
            return Result.Failure(
                ClassErrors.HasActiveEnrollments);
        }

        bool hasFutureSessions = await context.Sessions
            .AnyAsync(
                s => s.ClassId == command.Id &&
                     s.Status == SessionStatus.Scheduled &&
                     s.PlannedDatetime >= VietnamTime.UtcNow(),
                cancellationToken);

        if (hasFutureSessions)
        {
            return Result.Failure(
                ClassErrors.HasFutureSessions);
        }

        classEntity.Status = ClassStatus.Closed;
        classEntity.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

