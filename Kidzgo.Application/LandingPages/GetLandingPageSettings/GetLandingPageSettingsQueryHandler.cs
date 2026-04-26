using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.LandingPages.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LandingPages.GetLandingPageSettings;

public sealed class GetLandingPageSettingsQueryHandler(
    IDbContext context,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<GetLandingPageSettingsQuery, GetLandingPageSettingsResponse>
{
    public async Task<Result<GetLandingPageSettingsResponse>> Handle(
        GetLandingPageSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await context.LandingPageSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var content = await LandingPageReadModelBuilder.BuildAsync(
            context,
            schedulePatternParser,
            settings,
            publicOnly: false,
            cancellationToken);

        return Result.Success(new GetLandingPageSettingsResponse
        {
            LogoUrl = content.LogoUrl,
            FeaturedProgramsSection = content.FeaturedProgramsSection,
            FeaturedClassesSection = content.FeaturedClassesSection,
            FeaturedTeachersSection = content.FeaturedTeachersSection,
            FooterAddress = content.Footer.Address,
            FooterContactPhone = content.Footer.ContactPhone,
            FooterContactPhones = content.Footer.ContactPhones,
            FooterContactEmail = content.Footer.ContactEmail,
            FooterAddresses = content.Footer.Addresses,
            FooterSocialLinks = content.Footer.SocialLinks,
            FeaturedProgramIds = content.FeaturedProgramIds,
            FeaturedClassIds = content.FeaturedClassIds,
            FeaturedTeacherIds = content.FeaturedTeacherIds,
            FeaturedProgramConfigs = content.FeaturedProgramConfigs,
            FeaturedClassConfigs = content.FeaturedClassConfigs,
            FeaturedPrograms = content.FeaturedPrograms,
            FeaturedClasses = content.FeaturedClasses,
            FeaturedTeachers = content.FeaturedTeachers,
            UpdatedAt = content.UpdatedAt
        });
    }
}
