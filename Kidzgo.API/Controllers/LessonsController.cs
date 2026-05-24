using Kidzgo.API.Extensions;
using Kidzgo.Application.LessonPlans.GetLessonByCode;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/lessons")]
[ApiController]
[Authorize]
public sealed class LessonsController(ISender mediator) : ControllerBase
{
    [HttpGet("{lessonCode}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByCode(string lessonCode, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLessonByCodeQuery
        {
            LessonCode = lessonCode
        }, cancellationToken);

        return result.MatchOk();
    }
}
