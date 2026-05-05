using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LeaveRequests.ApproveLeaveRequest;
using Kidzgo.Application.LeaveRequests.BulkApproveLeaveRequests;
using Kidzgo.Application.LeaveRequests.CancelLeaveRequest;
using Kidzgo.Application.LeaveRequests.CreateLeaveRequest;
using Kidzgo.Application.LeaveRequests.GetLeaveRequestById;
using Kidzgo.Application.LeaveRequests.GetLeaveRequests;
using Kidzgo.Application.LeaveRequests.RejectLeaveRequest;
using Kidzgo.Domain.Sessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/leave-requests")]
[ApiController]
[Authorize]
public class LeaveRequestController : ControllerBase
{
    private readonly ISender _mediator;

    public LeaveRequestController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-091/092: Tạo Leave Request
    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateLeaveRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLeaveRequestCommand
        {
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId,
            SessionId = request.SessionId,
            SessionDate = request.SessionDate,
            EndDate = request.EndDate,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/leave-requests/{r.LeaveRequests.First().Id}");
    }

    /// UC-093: Danh sách Leave Requests
    [HttpGet]
    public async Task<IResult> GetList(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] LeaveRequestStatus? status,
        [FromQuery] Guid? branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeaveRequestsQuery
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

    /// UC-094: Chi tiết Leave Request
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetLeaveRequestByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-095: Duyệt Leave Request
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var command = new ApproveLeaveRequestCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("approve-bulk")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ApproveBulk(
        [FromBody] BulkApproveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BulkApproveLeaveRequestsCommand
        {
            Ids = request.Ids
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-096: Từ chối Leave Request
    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var command = new RejectLeaveRequestCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-097: Hủy Leave Request (Parent/Teacher/Admin)
    [HttpPut("{id:guid}/cancel")]
    public async Task<IResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelLeaveRequestCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

