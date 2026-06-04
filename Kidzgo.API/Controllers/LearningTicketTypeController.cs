using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LearningTicketTypes.CreateLearningTicketType;
using Kidzgo.Application.LearningTicketTypes.DeleteLearningTicketType;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypeById;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;
using Kidzgo.Application.LearningTicketTypes.UpdateLearningTicketType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/learning-ticket-types")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class LearningTicketTypeController : ControllerBase
{
    private readonly ISender _mediator;

    public LearningTicketTypeController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateLearningTicketTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateLearningTicketTypeCommand
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            CompatibilityMode = request.CompatibilityMode,
            AllowedDayGroups = request.AllowedDayGroups,
            AllowedTimeBands = request.AllowedTimeBands,
            AllowedTeacherTypes = request.AllowedTeacherTypes,
            AllowedUsageTypes = request.AllowedUsageTypes,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/learning-ticket-types/{x.Id}");
    }

    [HttpGet]
    public async Task<IResult> GetList(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLearningTicketTypesQuery
        {
            SearchTerm = searchTerm,
            IsActive = isActive
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLearningTicketTypeByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateLearningTicketTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateLearningTicketTypeCommand
        {
            Id = id,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            CompatibilityMode = request.CompatibilityMode,
            AllowedDayGroups = request.AllowedDayGroups,
            AllowedTimeBands = request.AllowedTimeBands,
            AllowedTeacherTypes = request.AllowedTeacherTypes,
            AllowedUsageTypes = request.AllowedUsageTypes,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteLearningTicketTypeCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }
}

