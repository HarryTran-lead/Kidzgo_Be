using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.SlotTypes.CreateSlotType;
using Kidzgo.Application.SlotTypes.DeleteSlotType;
using Kidzgo.Application.SlotTypes.GetSlotTypeById;
using Kidzgo.Application.SlotTypes.GetSlotTypes;
using Kidzgo.Application.SlotTypes.UpdateSlotType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/slot-types")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class SlotTypeController : ControllerBase
{
    private readonly ISender _mediator;

    public SlotTypeController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateSlotTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateSlotTypeCommand
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/slot-types/{x.Id}");
    }

    [HttpGet]
    public async Task<IResult> GetList(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSlotTypesQuery
        {
            SearchTerm = searchTerm,
            IsActive = isActive
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSlotTypeByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateSlotTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateSlotTypeCommand
        {
            Id = id,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSlotTypeCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }
}

