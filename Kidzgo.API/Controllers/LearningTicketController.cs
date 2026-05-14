using Kidzgo.API.Extensions;
using Kidzgo.Application.LearningTickets.GetStudentTicketBalance;
using Kidzgo.Application.LearningTickets.GetStudentTicketLedger;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/students/{studentProfileId:guid}/tickets")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff,Teacher")]
public class LearningTicketController : ControllerBase
{
    private readonly ISender _mediator;

    public LearningTicketController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("balance")]
    public async Task<IResult> GetBalance(
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStudentTicketBalanceQuery { StudentProfileId = studentProfileId },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("ledger")]
    public async Task<IResult> GetLedger(
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStudentTicketLedgerQuery { StudentProfileId = studentProfileId },
            cancellationToken);

        return result.MatchOk();
    }
}
