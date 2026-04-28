using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.UseMakeupCredit;

public sealed class UseMakeupCreditCommandHandler(
    IDbContext context,
    IUserContext userContext,
    SessionParticipantService sessionParticipantService)
    : ICommandHandler<UseMakeupCreditCommand>
{
    public async Task<Result> Handle(UseMakeupCreditCommand command, CancellationToken cancellationToken)
    {
        var nowUtc = VietnamTime.UtcNow();
        var today = VietnamTime.ToVietnamDateOnly(nowUtc);

        var studentProfileId = await ResolveStudentProfileIdAsync(command, cancellationToken);
        if (studentProfileId.IsFailure)
        {
            return Result.Failure(studentProfileId.Error);
        }

        var credit = await context.MakeupCredits
            .FirstOrDefaultAsync(c => c.Id == command.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.MakeupCreditId));
        }

        if (credit.StudentProfileId != studentProfileId.Value)
        {
            return Result.Failure(MakeupCreditErrors.NotBelongToStudent);
        }

        if (credit.ExpiresAt.HasValue && credit.ExpiresAt.Value <= nowUtc)
        {
            return Result.Failure(MakeupCreditErrors.Expired(command.MakeupCreditId));
        }

        Session? currentAllocatedSession = null;
        var canRescheduleUsedCredit = false;

        if (credit.UsedSessionId.HasValue)
        {
            currentAllocatedSession = await context.Sessions
                .AsNoTracking()
                .Include(s => s.Class)
                .ThenInclude(c => c.Program)
                .FirstOrDefaultAsync(s => s.Id == credit.UsedSessionId.Value, cancellationToken);

            if (currentAllocatedSession != null)
            {
                var allocatedDate = VietnamTime.ToVietnamDateOnly(currentAllocatedSession.PlannedDatetime);
                if (allocatedDate <= today)
                {
                    return Result.Failure(MakeupCreditErrors.CannotChangeAllocatedPastSession);
                }

                canRescheduleUsedCredit = true;
            }
        }

        if (credit.Status == MakeupCreditStatus.Expired)
        {
            return Result.Failure(MakeupCreditErrors.Expired(command.MakeupCreditId));
        }

        var isNewAllocation = credit.Status == MakeupCreditStatus.Available;
        var isFutureReschedule = credit.Status == MakeupCreditStatus.Used && canRescheduleUsedCredit;

        if (!isNewAllocation && !isFutureReschedule)
        {
            return Result.Failure(MakeupCreditErrors.NotAvailable(command.MakeupCreditId));
        }

        var sourceSession = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == credit.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(credit.SourceSessionId));
        }

        var targetSession = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .FirstOrDefaultAsync(s => s.Id == command.TargetSessionId, cancellationToken);

        if (targetSession is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.TargetSessionId));
        }

        var sourceDate = VietnamTime.ToVietnamDateOnly(sourceSession.PlannedDatetime);
        var targetDate = VietnamTime.ToVietnamDateOnly(targetSession.PlannedDatetime);

        if (targetDate < today)
        {
            return Result.Failure(MakeupCreditErrors.CannotUsePastDate);
        }

        if (!MakeupSessionRuleHelper.IsEligibleMakeupDate(sourceDate, targetDate))
        {
            return Result.Failure(MakeupCreditErrors.MustBeFutureWeek);
        }

        if (targetSession.ClassId != command.ClassId)
        {
            return Result.Failure(MakeupCreditErrors.SessionNotBelongToClass);
        }

        if (targetSession.Class.Program.IsMakeup != true)
        {
            return Result.Failure(MakeupCreditErrors.TargetClassMustBeMakeupProgram);
        }

        if (isFutureReschedule &&
            currentAllocatedSession is not null &&
            targetSession.Class.ProgramId != currentAllocatedSession.Class.ProgramId)
        {
            return Result.Failure(MakeupCreditErrors.MustStayInCurrentMakeupProgram);
        }

        if (isFutureReschedule &&
            currentAllocatedSession is not null &&
            currentAllocatedSession.Id == targetSession.Id)
        {
            return Result.Success();
        }

        var targetParticipants = await sessionParticipantService
            .GetParticipantsAsync(targetSession.Id, cancellationToken);

        if (targetParticipants.Any(p => p.StudentProfileId == studentProfileId.Value))
        {
            return Result.Failure(Error.Validation(
                "MakeupCredit.StudentAlreadyInTargetSession",
                "Student is already assigned to the target session."));
        }

        if (targetParticipants.Count >= targetSession.Class.Capacity)
        {
            return Result.Failure(MakeupCreditErrors.TargetSessionFull);
        }

        var targetWindowStartUtc = VietnamTime.TreatAsVietnamLocal(targetDate.AddDays(-1).ToDateTime(TimeOnly.MinValue));
        var targetWindowEndUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(targetDate.AddDays(1).ToDateTime(TimeOnly.MinValue)));

        var bookedSlots = await sessionParticipantService.GetStudentBookedSlotsAsync(
            studentProfileId.Value,
            targetWindowStartUtc,
            targetWindowEndUtc,
            cancellationToken);

        var currentAllocatedSlot = currentAllocatedSession is null
            ? ((DateTime Start, DateTime End)?)null
            : (currentAllocatedSession.PlannedDatetime,
                currentAllocatedSession.PlannedDatetime.AddMinutes(currentAllocatedSession.DurationMinutes));

        var targetStart = targetSession.PlannedDatetime;
        var targetEnd = targetSession.PlannedDatetime.AddMinutes(targetSession.DurationMinutes);
        var minGap = TimeSpan.FromHours(2);

        var hasConflict = bookedSlots
            .Where(slot => currentAllocatedSlot == null ||
                           slot.Start != currentAllocatedSlot.Value.Start ||
                           slot.End != currentAllocatedSlot.Value.End)
            .Any(slot =>
            {
                var overlap = targetStart < slot.End && targetEnd > slot.Start;
                var gapToStart = (targetStart - slot.End).Duration();
                var gapToEnd = (slot.Start - targetEnd).Duration();
                var tooClose = gapToStart < minGap || gapToEnd < minGap;
                return overlap || tooClose;
            });

        if (hasConflict)
        {
            return Result.Failure(Error.Validation(
                "MakeupCredit.TargetSessionConflict",
                "Target session conflicts with another session already assigned to the student."));
        }

        var existingAllocations = await context.MakeupAllocations
            .Where(a => a.MakeupCreditId == credit.Id && a.Status != MakeupAllocationStatus.Cancelled)
            .ToListAsync(cancellationToken);

        foreach (var existingAllocation in existingAllocations)
        {
            existingAllocation.Status = MakeupAllocationStatus.Cancelled;
        }

        credit.Status = MakeupCreditStatus.Used;
        credit.UsedSessionId = command.TargetSessionId;

        context.MakeupAllocations.Add(new MakeupAllocation
        {
            Id = Guid.NewGuid(),
            MakeupCreditId = credit.Id,
            TargetSessionId = command.TargetSessionId,
            Status = MakeupAllocationStatus.Pending,
            AssignedBy = userContext.UserId,
            AssignedAt = nowUtc,
            CreatedAt = nowUtc
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<Guid>> ResolveStudentProfileIdAsync(
        UseMakeupCreditCommand command,
        CancellationToken cancellationToken)
    {
        if (userContext.ParentId.HasValue)
        {
            if (!command.StudentProfileId.HasValue)
            {
                return Result.Failure<Guid>(MakeupCreditErrors.ParentMustProvideStudentProfileId);
            }

            var parentLink = await context.ParentStudentLinks
                .AnyAsync(
                    l => l.ParentProfileId == userContext.ParentId &&
                         l.StudentProfileId == command.StudentProfileId,
                    cancellationToken);

            if (!parentLink)
            {
                return Result.Failure<Guid>(MakeupCreditErrors.StudentNotBelongToParent);
            }

            return command.StudentProfileId.Value;
        }

        if (userContext.StudentId.HasValue)
        {
            return userContext.StudentId.Value;
        }

        return Result.Failure<Guid>(ProfileErrors.UserMustBeParentOrStudent);
    }
}
