using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.TeacherEvaluations.CreateTeacherEvaluation;
using Kidzgo.Application.AcademicProgression.TeacherEvaluations.GetTeacherEvaluationsByStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/teacher-evaluations")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff,Teacher")]
public class TeacherEvaluationController : ControllerBase
{
    private readonly ISender _mediator;

    public TeacherEvaluationController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateTeacherEvaluationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTeacherEvaluationCommand
        {
            StudentProfileId = request.StudentProfileId,
            ModuleId = request.ModuleId,
            Speaking = request.Speaking,
            Listening = request.Listening,
            Reading = request.Reading,
            Writing = request.Writing,
            Participation = request.Participation,
            Confidence = request.Confidence,
            Behavior = request.Behavior,
            Notes = request.Notes,
            EvaluatedAt = request.EvaluatedAt
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/teacher-evaluations/{x.Id}");
    }

    [HttpGet("{studentId:guid}")]
    public async Task<IResult> GetByStudent(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeacherEvaluationsByStudentQuery
        {
            StudentProfileId = studentId
        }, cancellationToken);

        return result.MatchOk();
    }
}
