using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.LandingPages.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LandingPages.GetLandingPage;

public sealed class GetLandingPageQueryHandler(
    IDbContext context,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<GetLandingPageQuery, GetLandingPageResponse>
{
    public async Task<Result<GetLandingPageResponse>> Handle(
        GetLandingPageQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await context.LandingPageSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var content = await LandingPageReadModelBuilder.BuildAsync(
            context,
            schedulePatternParser,
            settings,
            publicOnly: true,
            cancellationToken);

        return Result.Success(new GetLandingPageResponse
        {
            LogoUrl = content.LogoUrl,
            FeaturedProgramsSection = content.FeaturedProgramsSection,
            FeaturedClassesSection = content.FeaturedClassesSection,
            FeaturedTeachersSection = content.FeaturedTeachersSection,
            Footer = content.Footer,
            FeaturedPrograms = content.FeaturedPrograms,
            FeaturedClasses = content.FeaturedClasses,
            FeaturedTeachers = content.FeaturedTeachers,
            UpdatedAt = content.UpdatedAt
        });
    }
}
