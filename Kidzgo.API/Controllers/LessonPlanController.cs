using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LessonPlans.CreateLessonPlan;
using Kidzgo.Application.LessonPlans.GetClassLessonPlanSyllabus;
using Kidzgo.Application.LessonPlans.GetLessonPlanById;
using Kidzgo.Application.LessonPlans.UpdateLessonPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/lesson-plans")]
[ApiController]
[Authorize]
public class LessonPlanController : ControllerBase
{
    private readonly ISender _mediator;

    public LessonPlanController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateLessonPlan(
        [FromBody] CreateLessonPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLessonPlanCommand
        {
            ClassId = request.ClassId,
            SessionId = request.SessionId,
            TemplateId = request.TemplateId,
            PlannedContent = request.PlannedContent,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNotes = request.TeacherNotes,
            CompletionPercent = request.CompletionPercent,
            CarryForwardContent = request.CarryForwardContent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/lesson-plans/{r.Id}");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonPlanByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("classes/{classId:guid}/syllabus")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetClassLessonPlanSyllabus(
        Guid classId,
        CancellationToken cancellationToken)
    {
        var query = new GetClassLessonPlanSyllabusQuery
        {
            ClassId = classId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateLessonPlan(
        Guid id,
        [FromBody] UpdateLessonPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonPlanCommand
        {
            Id = id,
            TemplateId = request.TemplateId,
            PlannedContent = request.PlannedContent,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNotes = request.TeacherNotes,
            CompletionPercent = request.CompletionPercent,
            CarryForwardContent = request.CarryForwardContent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
