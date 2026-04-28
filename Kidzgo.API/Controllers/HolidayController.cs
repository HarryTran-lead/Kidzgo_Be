using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Holidays.CreateHoliday;
using Kidzgo.Application.Holidays.DeleteHoliday;
using Kidzgo.Application.Holidays.GetHolidayById;
using Kidzgo.Application.Holidays.GetHolidays;
using Kidzgo.Application.Holidays.ToggleHolidayStatus;
using Kidzgo.Application.Holidays.UpdateHoliday;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/holidays")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public sealed class HolidayController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetHolidays(
        [FromQuery] bool? isActive,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var query = new GetHolidaysQuery
        {
            IsActive = isActive,
            From = from,
            To = to
        };

        var result = await mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetHolidayById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetHolidayByIdQuery
        {
            Id = id
        };

        var result = await mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    public async Task<IResult> CreateHoliday(
        [FromBody] CreateHolidayRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateHolidayCommand
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
            IsActive = request.IsActive
        };

        var result = await mediator.Send(command, cancellationToken);
        return result.MatchCreated(holiday => $"/api/holidays/{holiday.Id}");
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateHoliday(
        Guid id,
        [FromBody] UpdateHolidayRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateHolidayCommand
        {
            Id = id,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
            IsActive = request.IsActive
        };

        var result = await mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IResult> ToggleHolidayStatus(Guid id, CancellationToken cancellationToken)
    {
        var command = new ToggleHolidayStatusCommand
        {
            Id = id
        };

        var result = await mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteHoliday(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteHolidayCommand
        {
            Id = id
        };

        var result = await mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
