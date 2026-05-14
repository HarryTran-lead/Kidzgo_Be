using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTickets.GetStudentTicketBalance;

public sealed class GetStudentTicketBalanceQueryHandler(
    IDbContext context)
    : IQueryHandler<GetStudentTicketBalanceQuery, GetStudentTicketBalanceResponse>
{
    public async Task<Result<GetStudentTicketBalanceResponse>> Handle(
        GetStudentTicketBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var available = await context.LearningTicketItems
            .CountAsync(
                x => x.StudentProfileId == query.StudentProfileId &&
                     x.Status == LearningTicketItemStatus.Available,
                cancellationToken);

        var consumed = await context.LearningTicketItems
            .CountAsync(
                x => x.StudentProfileId == query.StudentProfileId &&
                     x.Status == LearningTicketItemStatus.Consumed,
                cancellationToken);

        var totalGranted = await context.LearningTicketLedgers
            .Where(x => x.StudentProfileId == query.StudentProfileId &&
                        x.TransactionType == LearningTicketTransactionType.Grant)
            .SumAsync(x => x.Quantity, cancellationToken);

        return Result.Success(new GetStudentTicketBalanceResponse
        {
            StudentProfileId = query.StudentProfileId,
            Available = available,
            Consumed = consumed,
            TotalGranted = Math.Max(totalGranted, available + consumed)
        });
    }
}
