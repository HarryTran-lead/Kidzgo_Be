using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.ToggleProgramStatus;

public sealed class ToggleProgramStatusCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ToggleProgramStatusCommand, ToggleProgramStatusResponse>
{
    public async Task<Result<ToggleProgramStatusResponse>> Handle(ToggleProgramStatusCommand command, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure<ToggleProgramStatusResponse>(ProgramErrors.NotFound(command.Id));
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        bool oldIsActive = program.IsActive;
        bool newIsActive = !program.IsActive;

        if (oldIsActive && !newIsActive)
        {
            int activeClasses = await context.Classes
                .CountAsync(
                    c => c.ProgramId == program.Id &&
                         (c.Status == ClassStatus.Active || c.Status == ClassStatus.Planned),
                    cancellationToken);

            int activeStudents = await context.ClassEnrollments
                .Where(e => e.Class.ProgramId == program.Id &&
                            (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused))
                .Select(e => e.StudentProfileId)
                .Distinct()
                .CountAsync(cancellationToken);

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

            if (reasons.Count > 0)
            {
                AddAuditLog(
                    "RejectProgramStatusChange",
                    "Program",
                    program.Id,
                    new { isActive = oldIsActive },
                    new { isActive = newIsActive, blocked = true, reasons, counts });

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result.Failure<ToggleProgramStatusResponse>(
                    new StatusChangeBlockedError("Program", program.Id, reasons, counts));
            }
        }

        program.IsActive = !program.IsActive;
        program.UpdatedAt = VietnamTime.UtcNow();

        AddAuditLog(
            "UpdateProgramStatus",
            "Program",
            program.Id,
            new { isActive = oldIsActive },
            new { isActive = program.IsActive });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ToggleProgramStatusResponse
        {
            Id = program.Id,
            IsActive = program.IsActive
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

