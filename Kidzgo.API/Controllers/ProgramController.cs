using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Programs.AssignProgramToBranch;
using Kidzgo.Application.Programs.CreateProgram;
using Kidzgo.Application.Programs.DeleteProgram;
using Kidzgo.Application.Programs.GetCurriculumTree;
using Kidzgo.Application.Programs.GetProgramById;
using Kidzgo.Application.Programs.GetPrograms;
using Kidzgo.Application.Programs.ToggleProgramStatus;
using Kidzgo.Application.Programs.UpdateProgram;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/programs")]
[ApiController]
public class ProgramController : ControllerBase
{
    private readonly ISender _mediator;

    public ProgramController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-039: Tạo Program
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateProgram(
        [FromBody] CreateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProgramCommand
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsMakeup = request.IsMakeup,
            IsSupplementary = request.IsSupplementary
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(p => $"/api/programs/{p.Id}");
    }

    /// UC-040: Xem danh sách Programs
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetPrograms(
        [FromQuery] Guid? branchId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isMakeup,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramsQuery
        {
            BranchId = branchId,
            SearchTerm = searchTerm,
            IsActive = isActive,
            IsMakeup = isMakeup,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy danh sách tất cả Programs đang active (IsActive = true)
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IResult> GetActivePrograms(
        [FromQuery] Guid? branchId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramsQuery
        {
            BranchId = branchId,
            SearchTerm = searchTerm,
            IsActive = true,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-041: Xem chi tiết Program
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetProgramById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProgramByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-042: Cập nhật Program
    /// View curriculum tree for one program
    [HttpGet("{programId:guid}/curriculum-tree")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetCurriculumTree(
        Guid programId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgramCurriculumTreeQuery
        {
            ProgramId = programId
        }, cancellationToken);

        return result.MatchOk();
    }

    /// UC-042: Cap nhat Program
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateProgram(
        Guid id,
        [FromBody] UpdateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProgramCommand
        {
            Id = id,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsMakeup = request.IsMakeup,
            IsSupplementary = request.IsSupplementary
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/branches/{branchId:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AssignProgramToBranch(
        Guid id,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var command = new AssignProgramToBranchCommand
        {
            ProgramId = id,
            BranchId = branchId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(p => $"/api/programs/{p.ProgramId}/branches/{p.BranchId}");
    }

    /// UC-043: Xóa mềm Program
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteProgram(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProgramCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-044: Kích hoạt/Vô hiệu hóa Program
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ToggleProgramStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleProgramStatusCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
