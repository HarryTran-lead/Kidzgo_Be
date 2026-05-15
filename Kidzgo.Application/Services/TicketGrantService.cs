using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class TicketGrantService(IDbContext context)
{
    public Task GrantTicketsAsync(
        Guid studentProfileId,
        Guid registrationId,
        int quantity,
        Guid? learningTicketTypeId,
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
                LearningTicketTypeId = learningTicketTypeId,
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
