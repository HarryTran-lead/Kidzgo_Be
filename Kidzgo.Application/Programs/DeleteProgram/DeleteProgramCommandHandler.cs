using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.DeleteProgram;

public sealed class DeleteProgramCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteProgramCommand>
{
    public async Task<Result> Handle(DeleteProgramCommand command, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure(ProgramErrors.NotFound(command.Id));
        }

        bool hasActiveClasses = await context.Classes
            .AnyAsync(
                c => c.ProgramId == program.Id &&
                     (c.Status == ClassStatus.Active || c.Status == ClassStatus.Planned),
                cancellationToken);

        if (hasActiveClasses)
        {
            return Result.Failure(ProgramErrors.HasActiveClasses);
        }

        bool hasActiveEnrollments = await context.ClassEnrollments
            .AnyAsync(
                e => e.Class.ProgramId == program.Id &&
                     (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                cancellationToken);

        if (hasActiveEnrollments)
        {
            return Result.Failure(ProgramErrors.HasActiveEnrollments);
        }

        program.IsDeleted = true;
        program.IsActive = false;
        program.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

