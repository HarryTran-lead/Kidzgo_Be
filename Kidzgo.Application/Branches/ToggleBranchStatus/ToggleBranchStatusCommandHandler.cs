using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.ToggleBranchStatus;

public sealed class ToggleBranchStatusCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<ToggleBranchStatusCommand, ToggleBranchStatusResponse>
{
    public async Task<Result<ToggleBranchStatusResponse>> Handle(ToggleBranchStatusCommand command, CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<ToggleBranchStatusResponse>(BranchErrors.NotFound(command.Id));
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        bool oldIsActive = branch.IsActive;

        if (oldIsActive && !command.IsActive)
        {
            int activeClasses = await context.Classes
                .CountAsync(c => c.BranchId == branch.Id && c.Status == ClassStatus.Active, cancellationToken);

            int activeStudents = await context.ClassEnrollments
                .Where(e => e.Class.BranchId == branch.Id &&
                            (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused))
                .Select(e => e.StudentProfileId)
                .Distinct()
                .CountAsync(cancellationToken);

            int activeStaff = await context.Users
                .CountAsync(
                    u => u.BranchId == branch.Id &&
                         u.IsActive &&
                         !u.IsDeleted &&
                         (u.Role == UserRole.ManagementStaff ||
                          u.Role == UserRole.AccountantStaff ||
                          u.Role == UserRole.Teacher),
                    cancellationToken);

            int activeRooms = await context.Classrooms
                .CountAsync(c => c.BranchId == branch.Id && c.IsActive, cancellationToken);

            var reasons = new List<string>();
            var counts = new Dictionary<string, int>();

            if (activeClasses > 0)
            {
                reasons.Add("ACTIVE_CLASSES_EXIST");
                counts["activeClasses"] = activeClasses;
            }

            if (activeStudents > 0)
            {
                reasons.Add("ACTIVE_STUDENTS_EXIST");
                counts["activeStudents"] = activeStudents;
            }

            if (activeStaff > 0)
            {
                reasons.Add("ACTIVE_STAFF_EXIST");
                counts["activeStaff"] = activeStaff;
            }

            if (activeRooms > 0)
            {
                reasons.Add("ACTIVE_ROOMS_EXIST");
                counts["activeRooms"] = activeRooms;
            }

            if (reasons.Count > 0)
            {
                AddAuditLog(
                    "RejectBranchStatusChange",
                    "Branch",
                    branch.Id,
                    new { isActive = oldIsActive },
                    new { isActive = command.IsActive, blocked = true, reasons, counts });

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result.Failure<ToggleBranchStatusResponse>(
                    new StatusChangeBlockedError("Branch", branch.Id, reasons, counts));
            }
        }

        branch.IsActive = command.IsActive;
        branch.UpdatedAt = VietnamTime.UtcNow();

        AddAuditLog(
            "UpdateBranchStatus",
            "Branch",
            branch.Id,
            new { isActive = oldIsActive },
            new { isActive = branch.IsActive });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ToggleBranchStatusResponse
        {
            Id = branch.Id,
            IsActive = branch.IsActive
        };
    }

    private void AddAuditLog(string action, string entityType, Guid entityId, object dataBefore, object dataAfter)
    {
        context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = userContext.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DataBefore = JsonSerializer.Serialize(dataBefore),
            DataAfter = JsonSerializer.Serialize(dataAfter),
            IpAddress = userContext.IpAddress,
            CreatedAt = VietnamTime.UtcNow()
        });
    }
}

