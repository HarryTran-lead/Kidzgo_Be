using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.RegistrationDiscountCampaigns.CreateRegistrationDiscountCampaign;
using Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaignById;
using Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaigns;
using Kidzgo.Application.RegistrationDiscountCampaigns.ToggleRegistrationDiscountCampaignStatus;
using Kidzgo.Application.RegistrationDiscountCampaigns.UpdateRegistrationDiscountCampaign;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/registration-discount-campaigns")]
[ApiController]
public class RegistrationDiscountCampaignController : ControllerBase
{
    private readonly ISender _mediator;

    public RegistrationDiscountCampaignController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> CreateRegistrationDiscountCampaign(
        [FromBody] CreateRegistrationDiscountCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRegistrationDiscountCampaignCommand
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            LevelId = request.LevelId,
            TuitionPlanId = request.TuitionPlanId,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ApplyForInitialRegistration = request.ApplyForInitialRegistration,
            ApplyForRenewal = request.ApplyForRenewal,
            ApplyForUpgrade = request.ApplyForUpgrade
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(campaign => $"/api/registration-discount-campaigns/{campaign.Id}");
    }

    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRegistrationDiscountCampaigns(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] Guid? levelId,
        [FromQuery] Guid? tuitionPlanId,
        [FromQuery] bool? isActive,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRegistrationDiscountCampaignsQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            LevelId = levelId,
            TuitionPlanId = tuitionPlanId,
            IsActive = isActive,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRegistrationDiscountCampaignById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRegistrationDiscountCampaignByIdQuery
        {
            Id = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> UpdateRegistrationDiscountCampaign(
        Guid id,
        [FromBody] UpdateRegistrationDiscountCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRegistrationDiscountCampaignCommand
        {
            Id = id,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            LevelId = request.LevelId,
            TuitionPlanId = request.TuitionPlanId,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ApplyForInitialRegistration = request.ApplyForInitialRegistration,
            ApplyForRenewal = request.ApplyForRenewal,
            ApplyForUpgrade = request.ApplyForUpgrade
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> ToggleRegistrationDiscountCampaignStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleRegistrationDiscountCampaignStatusCommand
        {
            Id = id
        }, cancellationToken);

        return result.MatchOk();
    }
}
