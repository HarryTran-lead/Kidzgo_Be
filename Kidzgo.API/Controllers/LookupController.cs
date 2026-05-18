using Kidzgo.API.Extensions;
using Kidzgo.Application.Lookups.GetLookups;
using Kidzgo.Application.Programs.GetLevels;
using Kidzgo.Application.Sessions.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api")]
[ApiController]
public class LookupController : ControllerBase
{
    private readonly ISender _mediator;

    public LookupController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// Lấy danh sách Levels từ Programs
    [HttpGet("lookups/levels")]
    [Authorize]
    public async Task<IResult> GetLevels(CancellationToken cancellationToken)
    {
        var query = new GetLevelsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy danh sách Roles (SessionRoleType)
    [HttpGet("roles")]
    [Authorize]
    public async Task<IResult> GetRoles(CancellationToken cancellationToken)
    {
        var query = new GetRolesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy tất cả enum values cho lookups (attendanceStatus, sessionType, classStatus, etc.)
    [HttpGet("lookups")]
    [Authorize]
    public async Task<IResult> GetLookups(CancellationToken cancellationToken)
    {
        var query = new GetLookupsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

