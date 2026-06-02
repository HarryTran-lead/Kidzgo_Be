using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.TuitionPlans.CreatePackageCurriculumMapping;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/package-curriculum-mappings")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class PackageCurriculumMappingsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreatePackageCurriculumMappingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreatePackageCurriculumMappingCommand
        {
            TuitionPlanId = request.PackageId,
            SyllabusId = request.SyllabusId
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/packages/{x.TuitionPlanId}/syllabuses");
    }
}
