using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.RemoveProgramFromBranch;

public sealed class RemoveProgramFromBranchCommandHandler(IDbContext context)
    : ICommandHandler<RemoveProgramFromBranchCommand, RemoveProgramFromBranchResponse>
{
    public async Task<Result<RemoveProgramFromBranchResponse>> Handle(
        RemoveProgramFromBranchCommand command,
        CancellationToken cancellationToken)
    {
        var assignment = await context.BranchPrograms
            .FirstOrDefaultAsync(
                x => x.BranchId == command.BranchId && x.ProgramId == command.ProgramId,
                cancellationToken);

        if (assignment is null)
        {
            return Result.Failure<RemoveProgramFromBranchResponse>(
                ProgramErrors.BranchAssignmentNotFound(command.ProgramId, command.BranchId));
        }

        var hasOperationalClasses = await context.Classes.AnyAsync(
            x => x.BranchId == command.BranchId &&
                 x.ProgramId == command.ProgramId &&
                 x.Status != ClassStatus.Closed &&
                 x.Status != ClassStatus.Completed &&
                 x.Status != ClassStatus.Cancelled,
            cancellationToken);

        if (hasOperationalClasses)
        {
            return Result.Failure<RemoveProgramFromBranchResponse>(
                ProgramErrors.BranchAssignmentHasOperationalClasses(command.ProgramId, command.BranchId));
        }

        var hasActiveRegistrations = await context.Registrations.AnyAsync(
            x => x.BranchId == command.BranchId &&
                 x.ProgramId == command.ProgramId &&
                 x.Status != RegistrationStatus.Completed &&
                 x.Status != RegistrationStatus.Cancelled,
            cancellationToken);

        if (hasActiveRegistrations)
        {
            return Result.Failure<RemoveProgramFromBranchResponse>(
                ProgramErrors.BranchAssignmentHasActiveRegistrations(command.ProgramId, command.BranchId));
        }

        context.BranchPrograms.Remove(assignment);
        await context.SaveChangesAsync(cancellationToken);

        return new RemoveProgramFromBranchResponse
        {
            BranchId = command.BranchId,
            ProgramId = command.ProgramId
        };
    }
}
