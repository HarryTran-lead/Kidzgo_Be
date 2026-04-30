using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.PauseEnrollmentRequests.ApprovePauseEnrollmentRequest;
using Kidzgo.Application.PauseEnrollmentRequests.BulkApprovePauseEnrollmentRequests;
using Kidzgo.Application.PauseEnrollmentRequests.CancelPauseEnrollmentRequest;
using Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;
using Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequestById;
using Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequests;
using Kidzgo.Application.PauseEnrollmentRequests.ReassignEquivalentClass;
using Kidzgo.Application.PauseEnrollmentRequests.RejectPauseEnrollmentRequest;
using Kidzgo.Application.PauseEnrollmentRequests.UpdatePauseEnrollmentOutcome;
using Kidzgo.Domain.Classes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/pause-enrollment-requests")]
[ApiController]
[Authorize]
public class PauseEnrollmentRequestController : ControllerBase
{
    private readonly ISender _mediator;

    public PauseEnrollmentRequestController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreatePauseEnrollmentRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePauseEnrollmentRequestCommand
        {
            StudentProfileId = request.StudentProfileId,
            PauseFrom = request.PauseFrom,
            PauseTo = request.PauseTo,
            Reason = request.Reason,
            Scope = request.Scope,
            ClassId = request.ClassId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/pause-enrollment-requests/{r.Id}");
    }

    [HttpGet]
    public async Task<IResult> GetList(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] PauseEnrollmentRequestStatus? status,
        [FromQuery] Guid? branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPauseEnrollmentRequestsQuery
        {
            StudentProfileId = studentProfileId,
            ClassId = classId,
            Status = status,
            BranchId = branchId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPauseEnrollmentRequestByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var command = new ApprovePauseEnrollmentRequestCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("approve-bulk")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ApproveBulk(
        [FromBody] BulkApproveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BulkApprovePauseEnrollmentRequestsCommand
        {
            Ids = request.Ids
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var command = new RejectPauseEnrollmentRequestCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelPauseEnrollmentRequestCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}/outcome")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateOutcome(
        Guid id,
        [FromBody] UpdatePauseEnrollmentOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePauseEnrollmentOutcomeCommand
        {
            Id = id,
            Outcome = request.Outcome,
            OutcomeNote = request.OutcomeNote
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/reassign-equivalent-class")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ReassignEquivalentClass(
        Guid id,
        [FromBody] ReassignEquivalentClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReassignEquivalentClassCommand
        {
            PauseEnrollmentRequestId = id,
            RegistrationId = request.RegistrationId,
            NewClassId = request.NewClassId,
            Track = request.Track,
            WeeklyPattern = request.WeeklyPattern,
            EffectiveDate = request.EffectiveDate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
