using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.Modules.CreateModule;
using Kidzgo.Application.AcademicProgression.Modules.GetModules;
using Kidzgo.Application.AcademicProgression.Modules.UpdateModule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/modules")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class ModuleController : ControllerBase
{
    private readonly ISender _mediator;

    public ModuleController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IResult> Get(
        [FromQuery] Guid? levelId,
        [FromQuery] bool? isActive,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetModulesQuery
        {
            LevelId = levelId,
            IsActive = isActive,
            SearchTerm = searchTerm
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateModuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateModuleCommand
        {
            LevelId = request.LevelId,
            Code = request.Code,
            Name = request.Name,
            Order = request.Order,
            Description = request.Description,
            PlannedSessionCount = request.PlannedSessionCount,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/modules/{x.Id}");
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateModuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateModuleCommand
        {
            Id = id,
            Code = request.Code,
            Name = request.Name,
            Order = request.Order,
            Description = request.Description,
            PlannedSessionCount = request.PlannedSessionCount,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchOk();
    }
}
