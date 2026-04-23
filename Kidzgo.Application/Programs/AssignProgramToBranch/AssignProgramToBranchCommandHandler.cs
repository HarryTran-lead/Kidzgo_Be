using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.AssignProgramToBranch;

public sealed class AssignProgramToBranchCommandHandler(
    IDbContext context
) : ICommandHandler<AssignProgramToBranchCommand, AssignProgramToBranchResponse>
{
    public async Task<Result<AssignProgramToBranchResponse>> Handle(
        AssignProgramToBranchCommand command,
        CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<AssignProgramToBranchResponse>(ProgramErrors.BranchNotFound);
        }

        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (program is null)
        {
            return Result.Failure<AssignProgramToBranchResponse>(ProgramErrors.NotFound(command.ProgramId));
        }

        var assignmentExists = await context.BranchPrograms
            .AnyAsync(
                bp => bp.BranchId == command.BranchId && bp.ProgramId == command.ProgramId,
                cancellationToken);

        if (assignmentExists)
        {
            return Result.Failure<AssignProgramToBranchResponse>(
                ProgramErrors.AlreadyAssignedToBranch(command.ProgramId, command.BranchId));
        }

        var now = VietnamTime.UtcNow();
        var assignment = new BranchProgram
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            IsActive = true,
            DefaultMakeupClassId = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.BranchPrograms.Add(assignment);
        await context.SaveChangesAsync(cancellationToken);

        return new AssignProgramToBranchResponse
        {
            Id = assignment.Id,
            ProgramId = assignment.ProgramId,
            ProgramName = program.Name,
            BranchId = assignment.BranchId,
            BranchName = branch.Name,
            IsActive = assignment.IsActive,
            DefaultMakeupClassId = assignment.DefaultMakeupClassId
        };
    }
}
