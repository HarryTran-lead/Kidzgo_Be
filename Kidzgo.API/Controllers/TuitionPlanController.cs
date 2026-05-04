using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.TuitionPlans.CreateTuitionPlan;
using Kidzgo.Application.TuitionPlans.DeleteTuitionPlan;
using Kidzgo.Application.TuitionPlans.GetTuitionPlanById;
using Kidzgo.Application.TuitionPlans.GetTuitionPlans;
using Kidzgo.Application.TuitionPlans.ToggleTuitionPlanStatus;
using Kidzgo.Application.TuitionPlans.UpdateTuitionPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/tuition-plans")]
[ApiController]
public class TuitionPlanController : ControllerBase
{
    private readonly ISender _mediator;

    public TuitionPlanController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-045: Tạo Tuition Plan
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateTuitionPlan(
        [FromBody] CreateTuitionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTuitionPlanCommand
        {
            ProgramId = request.ProgramId,
            Name = request.Name,
            TotalSessions = request.TotalSessions,
            TuitionAmount = request.TuitionAmount,
            Currency = request.Currency
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(tp => $"/api/tuition-plans/{tp.Id}");
    }

    /// UC-046: Xem danh sách Tuition Plans
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetTuitionPlans(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTuitionPlansQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy tất cả Tuition Plans đang active (không cần authorize)
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IResult> GetActiveTuitionPlans(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTuitionPlansQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            IsActive = true, // Hardcode IsActive = true
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-047: Xem chi tiết Tuition Plan
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetTuitionPlanById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTuitionPlanByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-048: Cập nhật Tuition Plan
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateTuitionPlan(
        Guid id,
        [FromBody] UpdateTuitionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTuitionPlanCommand
        {
            Id = id,
            ProgramId = request.ProgramId,
            Name = request.Name,
            TotalSessions = request.TotalSessions,
            TuitionAmount = request.TuitionAmount,
            Currency = request.Currency
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-049: Xóa mềm Tuition Plan
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteTuitionPlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTuitionPlanCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-050: Kích hoạt/Vô hiệu hóa Tuition Plan
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ToggleTuitionPlanStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleTuitionPlanStatusCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

