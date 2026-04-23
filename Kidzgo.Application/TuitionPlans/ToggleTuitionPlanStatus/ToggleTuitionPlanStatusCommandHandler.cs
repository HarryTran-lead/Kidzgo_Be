using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.ToggleTuitionPlanStatus;

public sealed class ToggleTuitionPlanStatusCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ToggleTuitionPlanStatusCommand, ToggleTuitionPlanStatusResponse>
{
    public async Task<Result<ToggleTuitionPlanStatusResponse>> Handle(ToggleTuitionPlanStatusCommand command, CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<ToggleTuitionPlanStatusResponse>(TuitionPlanErrors.NotFound(command.Id));
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        bool oldIsActive = tuitionPlan.IsActive;
        bool newIsActive = !tuitionPlan.IsActive;

        if (oldIsActive && !newIsActive)
        {
            int activeEnrollments = await context.ClassEnrollments
                .CountAsync(
                    e => e.TuitionPlanId == tuitionPlan.Id &&
                         (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                    cancellationToken);

            if (activeEnrollments > 0)
            {
                var reasons = new List<string> { "ACTIVE_ENROLLMENTS_EXIST" };
                var counts = new Dictionary<string, int>
                {
                    ["activeEnrollments"] = activeEnrollments
                };

                AddAuditLog(
                    "RejectTuitionPlanStatusChange",
                    "TuitionPlan",
                    tuitionPlan.Id,
                    new { isActive = oldIsActive },
                    new { isActive = newIsActive, blocked = true, reasons, counts });

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result.Failure<ToggleTuitionPlanStatusResponse>(
                    new StatusChangeBlockedError("TuitionPlan", tuitionPlan.Id, reasons, counts));
            }
        }

        tuitionPlan.IsActive = !tuitionPlan.IsActive;
        tuitionPlan.UpdatedAt = VietnamTime.UtcNow();

        AddAuditLog(
            "UpdateTuitionPlanStatus",
            "TuitionPlan",
            tuitionPlan.Id,
            new { isActive = oldIsActive },
            new { isActive = tuitionPlan.IsActive });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ToggleTuitionPlanStatusResponse
        {
            Id = tuitionPlan.Id,
            IsActive = tuitionPlan.IsActive
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

