using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTickets.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTickets.GetStudentTicketLedger;

public sealed class GetStudentTicketLedgerQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentTicketLedgerQuery, GetStudentTicketLedgerResponse>
{
    public async Task<Result<GetStudentTicketLedgerResponse>> Handle(
        GetStudentTicketLedgerQuery query,
        CancellationToken cancellationToken)
    {
        var studentAccessResult = await LearningTicketAccessHelper.ResolveReadableStudentProfileIdAsync(
            context,
            userContext,
            query.StudentProfileId,
            cancellationToken);

        if (!studentAccessResult.IsSuccess)
        {
            return Result.Failure<GetStudentTicketLedgerResponse>(studentAccessResult.Error);
        }

        var studentProfileId = studentAccessResult.Value;

        var items = await context.LearningTicketLedgers
            .Where(x => x.StudentProfileId == studentProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new StudentTicketLedgerItemDto
            {
                Id = x.Id,
                TransactionType = x.TransactionType.ToString(),
                Quantity = x.Quantity,
                Reason = x.Reason,
                SessionId = x.SessionId,
                AttendanceId = x.AttendanceId,
                CreatedAt = VietnamTime.ToVietnamDateTime(x.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetStudentTicketLedgerResponse
        {
            Items = items
        });
    }
}
