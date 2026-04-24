using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.AssignBranch;

public sealed class AssignBranchCommandHandler(IDbContext context)
    : ICommandHandler<AssignBranchCommand, AssignBranchResponse>
{
    public async Task<Result<AssignBranchResponse>> Handle(AssignBranchCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AssignBranchResponse>(UserErrors.NotFound(command.UserId));
        }

        // Only ManagementStaff, AccountantStaff, Teacher, and Parent can have branch assigned
        if (user.Role != UserRole.Teacher && user.Role != UserRole.ManagementStaff && user.Role != UserRole.AccountantStaff && user.Role != UserRole.Parent)
        {
            return Result.Failure<AssignBranchResponse>(UserErrors.InvalidRoleForBranchAssignment);
        }

        if (command.BranchId.HasValue)
        {
            var branch = await context.Branches
                .FirstOrDefaultAsync(b => b.Id == command.BranchId.Value, cancellationToken);

            if (branch is null)
            {
                return Result.Failure<AssignBranchResponse>(BranchErrors.NotFound(command.BranchId));
            }

            if (!branch.IsActive)
            {
                return Result.Failure<AssignBranchResponse>(UserErrors.BranchInactive);
            }

            user.BranchId = command.BranchId.Value;
        }
        else
        {
            // Remove branch assignment
            user.BranchId = null;
        }

        user.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return new AssignBranchResponse
        {
            UserId = user.Id,
            BranchId = user.BranchId,
            BranchName = user.Branch?.Name
        };
    }
}

