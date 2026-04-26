using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LandingPages.GetLandingPage;
using Kidzgo.Application.LandingPages.GetLandingPageSettings;
using Kidzgo.Application.LandingPages.UpdateLandingPageSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/landing-page")]
[ApiController]
public class LandingPageController : ControllerBase
{
    private readonly ISender _mediator;

    public LandingPageController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetLandingPage(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLandingPageQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("settings")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetLandingPageSettings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLandingPageSettingsQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("settings")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateLandingPageSettings(
        [FromBody] UpdateLandingPageSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLandingPageSettingsCommand
        {
            LogoUrl = request.LogoUrl,
            FeaturedProgramsSectionTitle = request.FeaturedProgramsSectionTitle,
            FeaturedProgramsSectionSubtitle = request.FeaturedProgramsSectionSubtitle,
            FeaturedClassesSectionTitle = request.FeaturedClassesSectionTitle,
            FeaturedClassesSectionSubtitle = request.FeaturedClassesSectionSubtitle,
            FeaturedTeachersSectionTitle = request.FeaturedTeachersSectionTitle,
            FeaturedTeachersSectionSubtitle = request.FeaturedTeachersSectionSubtitle,
            FooterAddress = request.FooterAddress,
            FooterContactPhone = request.FooterContactPhone,
            FooterContactPhones = request.FooterContactPhones ?? [],
            FooterContactEmail = request.FooterContactEmail,
            FooterAddresses = request.FooterAddresses ?? [],
            FooterSocialLinks = request.FooterSocialLinks?.Select(link => new LandingPageFooterSocialLinkInput
            {
                Label = link.Label ?? string.Empty,
                Url = link.Url ?? string.Empty,
                IconKey = link.IconKey
            }).ToList() ?? [],
            FeaturedPrograms = request.FeaturedPrograms?.Select(item => new LandingPageFeaturedItemInput
            {
                Id = item.Id,
                Tags = item.Tags ?? []
            }).ToList() ?? request.FeaturedProgramIds?.Select(id => new LandingPageFeaturedItemInput
            {
                Id = id
            }).ToList() ?? [],
            FeaturedClasses = request.FeaturedClasses?.Select(item => new LandingPageFeaturedItemInput
            {
                Id = item.Id,
                Tags = item.Tags ?? []
            }).ToList() ?? request.FeaturedClassIds?.Select(id => new LandingPageFeaturedItemInput
            {
                Id = id
            }).ToList() ?? [],
            FeaturedTeacherIds = request.FeaturedTeacherIds ?? []
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
