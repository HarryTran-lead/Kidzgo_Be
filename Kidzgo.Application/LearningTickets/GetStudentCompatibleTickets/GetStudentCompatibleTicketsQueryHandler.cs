using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTickets.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTickets.GetStudentCompatibleTickets;

public sealed class GetStudentCompatibleTicketsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentCompatibleTicketsQuery, GetStudentCompatibleTicketsResponse>
{
    public async Task<Result<GetStudentCompatibleTicketsResponse>> Handle(
        GetStudentCompatibleTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var studentAccessResult = await LearningTicketAccessHelper.ResolveReadableStudentProfileIdAsync(
            context,
            userContext,
            query.StudentProfileId,
            cancellationToken);

        if (!studentAccessResult.IsSuccess)
        {
            return Result.Failure<GetStudentCompatibleTicketsResponse>(studentAccessResult.Error);
        }

        var ticketItem = await context.LearningTicketItems
            .AsNoTracking()
            .Where(x => x.StudentProfileId == studentAccessResult.Value &&
                        x.Status == LearningTicketItemStatus.Available)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new GetStudentCompatibleTicketsResponse
        {
            Compatible = ticketItem is not null,
            TicketItemId = ticketItem?.Id,
            TicketTypeId = null,
            TicketTypeCode = null,
            Reason = ticketItem is not null
                ? "Available ticket found"
                : "No available tickets"
        });
    }
}
