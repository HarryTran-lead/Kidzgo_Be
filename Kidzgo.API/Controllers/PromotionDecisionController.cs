using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.AcademicProgression.PromotionDecisions.CreatePromotionDecision;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/promotion-decisions")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff,Teacher")]
public class PromotionDecisionController : ControllerBase
{
    private readonly ISender _mediator;

    public PromotionDecisionController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreatePromotionDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreatePromotionDecisionCommand
        {
            StudentProfileId = request.StudentProfileId,
            ModuleId = request.ModuleId,
            Reason = request.Reason,
            ApprovedAt = request.ApprovedAt
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/promotion-decisions/{x.Id}");
    }
}
