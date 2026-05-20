using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LessonPlanUnits.DeleteLessonPlanUnit;
using Kidzgo.Application.LessonPlanUnits.ReorderLessonPlanUnitLessons;
using Kidzgo.Application.LessonPlanUnits.UpdateLessonPlanUnit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/lesson-plan-units")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class LessonPlanUnitController(ISender mediator) : ControllerBase
{
    [HttpPatch("{id:guid}")]
    public async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateLessonPlanUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateLessonPlanUnitCommand
        {
            Id = id,
            Name = request.Name,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteLessonPlanUnitCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/lessons/reorder")]
    public async Task<IResult> ReorderLessons(
        Guid id,
        [FromBody] IReadOnlyList<ReorderLessonPlanUnitLessonRequest> request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReorderLessonPlanUnitLessonsCommand
        {
            UnitId = id,
            Items = request
                .Select(x => new ReorderLessonPlanUnitLessonItem
                {
                    Id = x.Id,
                    OrderIndexInUnit = x.OrderIndexInUnit
                })
                .ToList()
        }, cancellationToken);

        return result.MatchOk();
    }
}
