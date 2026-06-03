using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTickets.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.LearningTickets.GetStudentCompatibleTickets;

public sealed class GetStudentCompatibleTicketsQueryHandler(
    IDbContext context,
    IUserContext userContext,
    TicketCompatibilityService ticketCompatibilityService)
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

        var selection = await ticketCompatibilityService.ValidateStudentSessionCompatibilityAsync(
            studentAccessResult.Value,
            query.SessionId,
            cancellationToken);

        return Result.Success(new GetStudentCompatibleTicketsResponse
        {
            Compatible = selection.IsCompatible && selection.TicketItem is not null,
            TicketItemId = selection.TicketItem?.Id,
            TicketTypeId = selection.TicketTypeId,
            TicketTypeCode = selection.TicketTypeCode,
            Reason = selection.Reason
        });
    }
}
