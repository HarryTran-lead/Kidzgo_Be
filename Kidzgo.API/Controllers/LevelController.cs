using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.Levels.CreateLevel;
using Kidzgo.Application.AcademicProgression.Levels.GetLevels;
using Kidzgo.Application.AcademicProgression.Levels.UpdateLevel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/levels")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class LevelController : ControllerBase
{
    private readonly ISender _mediator;

    public LevelController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IResult> Get(
        [FromQuery] Guid? programId,
        [FromQuery] bool? isActive,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLevelsQuery
        {
            ProgramId = programId,
            IsActive = isActive,
            SearchTerm = searchTerm
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateLevelCommand
        {
            ProgramId = request.ProgramId,
            Code = request.Code,
            Name = request.Name,
            Order = request.Order,
            Description = request.Description,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/levels/{x.Id}");
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateLevelCommand
        {
            Id = id,
            Code = request.Code,
            Name = request.Name,
            Order = request.Order,
            Description = request.Description,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchOk();
    }
}
