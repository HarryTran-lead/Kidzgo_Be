using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTickets.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTickets.GetStudentTicketBalance;

public sealed class GetStudentTicketBalanceQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentTicketBalanceQuery, GetStudentTicketBalanceResponse>
{
    public async Task<Result<GetStudentTicketBalanceResponse>> Handle(
        GetStudentTicketBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var studentAccessResult = await LearningTicketAccessHelper.ResolveReadableStudentProfileIdAsync(
            context,
            userContext,
            query.StudentProfileId,
            cancellationToken);

        if (!studentAccessResult.IsSuccess)
        {
            return Result.Failure<GetStudentTicketBalanceResponse>(studentAccessResult.Error);
        }

        var studentProfileId = studentAccessResult.Value;

        var available = await context.LearningTicketItems
            .CountAsync(
                x => x.StudentProfileId == studentProfileId &&
                     x.Status == LearningTicketItemStatus.Available,
                cancellationToken);

        var consumed = await context.LearningTicketItems
            .CountAsync(
                x => x.StudentProfileId == studentProfileId &&
                     x.Status == LearningTicketItemStatus.Consumed,
                cancellationToken);

        var totalGranted = await context.LearningTicketLedgers
            .Where(x => x.StudentProfileId == studentProfileId &&
                        x.TransactionType == LearningTicketTransactionType.Grant)
            .SumAsync(x => x.Quantity, cancellationToken);

        return Result.Success(new GetStudentTicketBalanceResponse
        {
            StudentProfileId = studentProfileId,
            Available = available,
            Consumed = consumed,
            TotalGranted = Math.Max(totalGranted, available + consumed)
        });
    }
}
