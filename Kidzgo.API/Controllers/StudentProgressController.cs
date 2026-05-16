using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.StudentProgress.GetAcademicDashboard;
using Kidzgo.Application.AcademicProgression.StudentProgress.GetStudentProgress;
using Kidzgo.Application.AcademicProgression.StudentProgress.UpdateStudentProgress;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/student-progress")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff,Teacher")]
public class StudentProgressController : ControllerBase
{
    private readonly ISender _mediator;

    public StudentProgressController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{studentId:guid}")]
    public async Task<IResult> Get(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentProgressQuery
        {
            StudentProfileId = studentId
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("dashboard")]
    public async Task<IResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAcademicDashboardQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("update")]
    public async Task<IResult> Update([FromBody] UpdateStudentProgressRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateStudentProgressCommand
        {
            StudentProfileId = request.StudentProfileId,
            ModuleId = request.ModuleId,
            CurrentLessonPlanTemplateId = request.CurrentLessonPlanTemplateId,
            CompletionPercent = request.CompletionPercent
        }, cancellationToken);

        return result.MatchOk();
    }
}
