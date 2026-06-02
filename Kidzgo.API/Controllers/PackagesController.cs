using Kidzgo.API.Extensions;
using Kidzgo.Application.TuitionPlans.GetPackageSyllabuses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/packages")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class PackagesController(ISender mediator) : ControllerBase
{
    [HttpGet("{id:guid}/syllabuses")]
    public async Task<IResult> GetSyllabuses(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPackageSyllabusesQuery
        {
            TuitionPlanId = id
        }, cancellationToken);

        return result.MatchOk();
    }
}
