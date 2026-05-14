using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTickets.GetStudentTicketLedger;

public sealed class GetStudentTicketLedgerQueryHandler(
    IDbContext context)
    : IQueryHandler<GetStudentTicketLedgerQuery, GetStudentTicketLedgerResponse>
{
    public async Task<Result<GetStudentTicketLedgerResponse>> Handle(
        GetStudentTicketLedgerQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.LearningTicketLedgers
            .Where(x => x.StudentProfileId == query.StudentProfileId)
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
