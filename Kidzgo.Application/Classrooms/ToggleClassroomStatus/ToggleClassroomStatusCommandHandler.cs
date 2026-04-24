using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classrooms.ToggleClassroomStatus;

public sealed class ToggleClassroomStatusCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ToggleClassroomStatusCommand, ToggleClassroomStatusResponse>
{
    public async Task<Result<ToggleClassroomStatusResponse>> Handle(ToggleClassroomStatusCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classroom is null)
        {
            return Result.Failure<ToggleClassroomStatusResponse>(ClassroomErrors.NotFound(command.Id));
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        bool oldIsActive = classroom.IsActive;
        bool newIsActive = !classroom.IsActive;

        if (oldIsActive && !newIsActive)
        {
            var now = VietnamTime.UtcNow();

            int activeClasses = await context.Classes
                .CountAsync(
                    c => c.RoomId == classroom.Id && c.Status == ClassStatus.Active,
                    cancellationToken);

            int futureSessions = await context.Sessions
                .CountAsync(
                    s => (s.PlannedRoomId == classroom.Id || s.ActualRoomId == classroom.Id) &&
                         s.Status != SessionStatus.Cancelled &&
                         s.PlannedDatetime >= now,
                    cancellationToken);

            var reasons = new List<string>();
            var counts = new Dictionary<string, int>();

            if (activeClasses > 0)
            {
                reasons.Add("ACTIVE_CLASSES_EXIST");
                counts["activeClasses"] = activeClasses;
            }

            if (futureSessions > 0)
            {
                reasons.Add("FUTURE_SESSIONS_EXIST");
                counts["futureSessions"] = futureSessions;
            }

            if (reasons.Count > 0)
            {
                AddAuditLog(
                    "RejectClassroomStatusChange",
                    "Classroom",
                    classroom.Id,
                    new { isActive = oldIsActive },
                    new { isActive = newIsActive, blocked = true, reasons, counts });

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result.Failure<ToggleClassroomStatusResponse>(
                    new StatusChangeBlockedError("Classroom", classroom.Id, reasons, counts));
            }
        }

        classroom.IsActive = !classroom.IsActive;

        AddAuditLog(
            "UpdateClassroomStatus",
            "Classroom",
            classroom.Id,
            new { isActive = oldIsActive },
            new { isActive = classroom.IsActive });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ToggleClassroomStatusResponse
        {
            Id = classroom.Id,
            IsActive = classroom.IsActive
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

