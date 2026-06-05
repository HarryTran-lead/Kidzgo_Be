using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTickets.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
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
                     x.Status == LearningTicketItemStatus.Available &&
                     x.Registration.Status != RegistrationStatus.Cancelled &&
                     x.Registration.Status != RegistrationStatus.Completed,
                cancellationToken);

        var consumed = await context.LearningTicketItems
            .CountAsync(
                x => x.StudentProfileId == studentProfileId &&
                     x.Status == LearningTicketItemStatus.Consumed &&
                     x.Registration.Status != RegistrationStatus.Cancelled &&
                     x.Registration.Status != RegistrationStatus.Completed,
                cancellationToken);

        return Result.Success(new GetStudentTicketBalanceResponse
        {
            StudentProfileId = studentProfileId,
            Available = available,
            Consumed = consumed,
            TotalGranted = available + consumed
        });
    }
}
