using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.RemedialPlans.CreateRemedialPlan;
using Kidzgo.Application.AcademicProgression.RemedialPlans.GetRemedialPlansByStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/remedial-plans")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff,Teacher")]
public class RemedialPlanController : ControllerBase
{
    private readonly ISender _mediator;

    public RemedialPlanController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateRemedialPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRemedialPlanCommand
        {
            StudentProfileId = request.StudentProfileId,
            ModuleId = request.ModuleId,
            WeakSkills = request.WeakSkills,
            RecommendedSessionCount = request.RecommendedSessionCount,
            Notes = request.Notes
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/remedial-plans/{x.Id}");
    }

    [HttpGet("{studentId:guid}")]
    public async Task<IResult> GetByStudent(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRemedialPlansByStudentQuery
        {
            StudentProfileId = studentId
        }, cancellationToken);

        return result.MatchOk();
    }
}
