using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.DeleteBranch;

public sealed class DeleteBranchCommandHandler(IDbContext context)
    : ICommandHandler<DeleteBranchCommand>
{
    public async Task<Result> Handle(DeleteBranchCommand command, CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (branch is null)
        {
            return Result.Failure(BranchErrors.NotFound(command.Id));
        }

        bool hasActiveDependencies = await context.Classes.AnyAsync(
                                         c => c.BranchId == branch.Id && c.Status == ClassStatus.Active,
                                         cancellationToken) ||
                                     await context.ClassEnrollments.AnyAsync(
                                         e => e.Class.BranchId == branch.Id &&
                                              (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                                         cancellationToken) ||
                                     await context.Users.AnyAsync(
                                         u => u.BranchId == branch.Id &&
                                              u.IsActive &&
                                              !u.IsDeleted &&
                                              (u.Role == UserRole.ManagementStaff ||
                                               u.Role == UserRole.AccountantStaff ||
                                               u.Role == UserRole.Teacher),
                                         cancellationToken) ||
                                     await context.Classrooms.AnyAsync(
                                         r => r.BranchId == branch.Id && r.IsActive,
                                         cancellationToken);

        if (hasActiveDependencies)
        {
            return Result.Failure(BranchErrors.HasActiveDependencies);
        }

        branch.IsActive = false;
        branch.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

