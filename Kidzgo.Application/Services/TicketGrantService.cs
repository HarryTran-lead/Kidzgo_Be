using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class TicketGrantService(IDbContext context)
{
    public Task GrantTicketsAsync(
        Guid studentProfileId,
        Guid registrationId,
        int quantity,
        string reason,
        LearningTicketSource source,
        Guid? createdByUserId,
        CancellationToken cancellationToken)
    {
        if (quantity <= 0)
        {
            return Task.CompletedTask;
        }

        var now = VietnamTime.UtcNow();

        var ticketItems = Enumerable.Range(0, quantity)
            .Select(_ => new LearningTicketItem
            {
                Id = Guid.NewGuid(),
                StudentProfileId = studentProfileId,
                RegistrationId = registrationId,
                Status = LearningTicketItemStatus.Available,
                Source = source,
                CreatedAt = now
            })
            .ToList();

        context.LearningTicketItems.AddRange(ticketItems);

        context.LearningTicketLedgers.Add(new LearningTicketLedger
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            RegistrationId = registrationId,
            TransactionType = LearningTicketTransactionType.Grant,
            Quantity = quantity,
            Reason = reason,
            CreatedByUserId = createdByUserId,
            CreatedAt = now
        });

        return Task.CompletedTask;
    }

    public async Task<int> GrantRolloverMakeupCreditsAsync(
        Guid studentProfileId,
        Guid registrationId,
        Guid? createdByUserId,
        CancellationToken cancellationToken)
    {
        var rolloverCredits = await context.MakeupCredits
            .Where(x => x.StudentProfileId == studentProfileId &&
                        x.Status == MakeupCreditStatus.Available)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (rolloverCredits.Count == 0)
        {
            return 0;
        }

        foreach (var credit in rolloverCredits)
        {
            credit.Status = MakeupCreditStatus.Transferred;
            credit.ExpiresAt = null;
            credit.UsedSessionId = null;
        }

        await GrantTicketsAsync(
            studentProfileId,
            registrationId,
            rolloverCredits.Count,
            "Rollover available makeup credits",
            LearningTicketSource.Rollover,
            createdByUserId,
            cancellationToken);

        return rolloverCredits.Count;
    }

    public async Task<int> VoidAvailableTicketsAsync(
        Guid studentProfileId,
        Guid registrationId,
        string reason,
        Guid? createdByUserId,
        CancellationToken cancellationToken)
    {
        var availableItems = await context.LearningTicketItems
            .Where(x => x.StudentProfileId == studentProfileId &&
                        x.RegistrationId == registrationId &&
                        x.Status == LearningTicketItemStatus.Available)
            .ToListAsync(cancellationToken);

        if (availableItems.Count == 0)
        {
            return 0;
        }

        var now = VietnamTime.UtcNow();

        foreach (var item in availableItems)
        {
            item.Status = LearningTicketItemStatus.Voided;
            item.ConsumedBySessionId = null;
            item.ConsumedByAttendanceId = null;
            item.ConsumedAt = null;
        }

        context.LearningTicketLedgers.Add(new LearningTicketLedger
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            RegistrationId = registrationId,
            TransactionType = LearningTicketTransactionType.Void,
            Quantity = -availableItems.Count,
            Reason = reason,
            CreatedByUserId = createdByUserId,
            CreatedAt = now
        });

        return availableItems.Count;
    }

    public async Task<int> GetAvailableTicketsAsync(
        Guid registrationId,
        CancellationToken cancellationToken)
    {
        return await context.LearningTicketItems
            .CountAsync(
                x => x.RegistrationId == registrationId &&
                     x.Status == LearningTicketItemStatus.Available,
                cancellationToken);
    }

    public async Task SyncRegistrationSessionCacheAsync(
        Guid registrationId,
        DateTime? nowOverrideUtc,
        CancellationToken cancellationToken)
    {
        var registration = await context.Registrations
            .FirstOrDefaultAsync(x => x.Id == registrationId, cancellationToken);

        if (registration is null)
        {
            return;
        }

        var available = await context.LearningTicketItems
            .CountAsync(
                x => x.RegistrationId == registrationId &&
                     x.Status == LearningTicketItemStatus.Available,
                cancellationToken);

        var consumed = await context.LearningTicketItems
            .CountAsync(
                x => x.RegistrationId == registrationId &&
                     x.Status == LearningTicketItemStatus.Consumed,
                cancellationToken);

        registration.UsedSessions = consumed;
        registration.RemainingSessions = available;
        registration.TotalSessions = consumed + available;
        registration.UpdatedAt = nowOverrideUtc ?? VietnamTime.UtcNow();
    }
}
