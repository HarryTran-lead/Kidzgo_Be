using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.TicketTypeCompatibilities.CreateTicketTypeCompatibility;
using Kidzgo.Application.TicketTypeCompatibilities.DeleteTicketTypeCompatibility;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilityById;
using Kidzgo.Application.TicketTypeCompatibilities.UpdateTicketTypeCompatibility;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/ticket-type-compatibilities")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class TicketTypeCompatibilityController : ControllerBase
{
    private readonly ISender _mediator;

    public TicketTypeCompatibilityController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateTicketTypeCompatibilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTicketTypeCompatibilityCommand
        {
            LearningTicketTypeId = request.LearningTicketTypeId,
            SlotTypeId = request.SlotTypeId,
            IsCompatible = request.IsCompatible
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/ticket-type-compatibilities/{x.Id}");
    }

    [HttpGet]
    public async Task<IResult> GetList(
        [FromQuery] Guid? learningTicketTypeId,
        [FromQuery] Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTicketTypeCompatibilitiesQuery
        {
            LearningTicketTypeId = learningTicketTypeId,
            SlotTypeId = slotTypeId
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTicketTypeCompatibilityByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateTicketTypeCompatibilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateTicketTypeCompatibilityCommand
        {
            Id = id,
            LearningTicketTypeId = request.LearningTicketTypeId,
            SlotTypeId = request.SlotTypeId,
            IsCompatible = request.IsCompatible
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTicketTypeCompatibilityCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }
}