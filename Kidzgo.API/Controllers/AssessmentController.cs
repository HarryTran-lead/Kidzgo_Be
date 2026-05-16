using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.Assessments.CreateAssessment;
using Kidzgo.Application.AcademicProgression.Assessments.GetAssessmentsByStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/assessments")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff,Teacher")]
public class AssessmentController : ControllerBase
{
    private readonly ISender _mediator;

    public AssessmentController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateAssessmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateAssessmentCommand
        {
            StudentProfileId = request.StudentProfileId,
            ModuleId = request.ModuleId,
            SessionId = request.SessionId,
            Type = request.Type,
            Score = request.Score,
            TeacherComment = request.TeacherComment,
            AssessedAt = request.AssessedAt
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/assessments/{x.Id}");
    }

    [HttpGet("{studentId:guid}")]
    public async Task<IResult> GetByStudent(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAssessmentsByStudentQuery
        {
            StudentProfileId = studentId
        }, cancellationToken);

        return result.MatchOk();
    }
}
