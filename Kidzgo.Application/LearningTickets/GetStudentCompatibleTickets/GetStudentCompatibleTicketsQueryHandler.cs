using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.LearningTickets.GetStudentCompatibleTickets;

public sealed class GetStudentCompatibleTicketsQueryHandler(
    TicketCompatibilityService ticketCompatibilityService)
    : IQueryHandler<GetStudentCompatibleTicketsQuery, GetStudentCompatibleTicketsResponse>
{
    public async Task<Result<GetStudentCompatibleTicketsResponse>> Handle(
        GetStudentCompatibleTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var selection = await ticketCompatibilityService.ValidateStudentSessionCompatibilityAsync(
            query.StudentProfileId,
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
