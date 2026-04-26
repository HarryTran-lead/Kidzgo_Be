using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.LandingPages.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Website;
using Kidzgo.Domain.Website.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LandingPages.UpdateLandingPageSettings;

public sealed class UpdateLandingPageSettingsCommandHandler(
    IDbContext context,
    ISchedulePatternParser schedulePatternParser
) : ICommandHandler<UpdateLandingPageSettingsCommand, UpdateLandingPageSettingsResponse>
{
    public async Task<Result<UpdateLandingPageSettingsResponse>> Handle(
        UpdateLandingPageSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var featuredProgramConfigs = command.FeaturedPrograms
            .Where(item => item.Id != Guid.Empty)
            .GroupBy(item => item.Id)
            .Select(group => new LandingPageFeaturedItemConfigDto
            {
                Id = group.Key,
                Tags = group.First().Tags
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Select(tag => tag.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .ToArray();
        var featuredClassConfigs = command.FeaturedClasses
            .Where(item => item.Id != Guid.Empty)
            .GroupBy(item => item.Id)
            .Select(group => new LandingPageFeaturedItemConfigDto
            {
                Id = group.Key,
                Tags = group.First().Tags
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Select(tag => tag.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .ToArray();
        var featuredProgramIds = featuredProgramConfigs
            .Select(item => item.Id)
            .ToArray();
        var featuredClassIds = featuredClassConfigs
            .Select(item => item.Id)
            .ToArray();
        var featuredTeacherIds = command.FeaturedTeacherIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
        var footerAddresses = command.FooterAddresses
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(address => address.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var footerContactPhones = command.FooterContactPhones
            .Where(phone => !string.IsNullOrWhiteSpace(phone))
            .Select(phone => phone.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var footerSocialLinks = command.FooterSocialLinks
            .Where(link => !string.IsNullOrWhiteSpace(link.Label) && !string.IsNullOrWhiteSpace(link.Url))
            .Select(link => new LandingPageFooterSocialLinkDto
            {
                Label = link.Label.Trim(),
                Url = link.Url.Trim(),
                IconKey = NormalizeOptional(link.IconKey)
            })
            .GroupBy(link => $"{link.Label}|{link.Url}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        var programIds = await context.Programs
            .AsNoTracking()
            .Where(program => featuredProgramIds.Contains(program.Id) &&
                              !program.IsDeleted)
            .Select(program => program.Id)
            .ToListAsync(cancellationToken);

        var missingProgramIds = featuredProgramIds.Except(programIds).ToArray();
        if (missingProgramIds.Length > 0)
        {
            return Result.Failure<UpdateLandingPageSettingsResponse>(
                LandingPageErrors.InvalidFeaturedPrograms(missingProgramIds));
        }

        var classIds = await context.Classes
            .AsNoTracking()
            .Where(classEntity => featuredClassIds.Contains(classEntity.Id))
            .Select(classEntity => classEntity.Id)
            .ToListAsync(cancellationToken);

        var missingClassIds = featuredClassIds.Except(classIds).ToArray();
        if (missingClassIds.Length > 0)
        {
            return Result.Failure<UpdateLandingPageSettingsResponse>(
                LandingPageErrors.InvalidFeaturedClasses(missingClassIds));
        }

        var teacherIds = await context.Users
            .AsNoTracking()
            .Where(user => featuredTeacherIds.Contains(user.Id) &&
                           user.Role == UserRole.Teacher &&
                           !user.IsDeleted)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        var missingTeacherIds = featuredTeacherIds.Except(teacherIds).ToArray();
        if (missingTeacherIds.Length > 0)
        {
            return Result.Failure<UpdateLandingPageSettingsResponse>(
                LandingPageErrors.InvalidFeaturedTeachers(missingTeacherIds));
        }

        var settings = await context.LandingPageSettings.FirstOrDefaultAsync(cancellationToken);
        var now = VietnamTime.UtcNow();

        if (settings is null)
        {
            settings = new LandingPageSettings
            {
                Id = 1,
                CreatedAt = now
            };

            context.LandingPageSettings.Add(settings);
        }

        settings.LogoUrl = NormalizeOptional(command.LogoUrl);
        settings.FeaturedProgramsSectionTitle = NormalizeOptional(command.FeaturedProgramsSectionTitle);
        settings.FeaturedProgramsSectionSubtitle = NormalizeOptional(command.FeaturedProgramsSectionSubtitle);
        settings.FeaturedClassesSectionTitle = NormalizeOptional(command.FeaturedClassesSectionTitle);
        settings.FeaturedClassesSectionSubtitle = NormalizeOptional(command.FeaturedClassesSectionSubtitle);
        settings.FeaturedTeachersSectionTitle = NormalizeOptional(command.FeaturedTeachersSectionTitle);
        settings.FeaturedTeachersSectionSubtitle = NormalizeOptional(command.FeaturedTeachersSectionSubtitle);
        settings.FooterAddress = NormalizeOptional(command.FooterAddress) ?? footerAddresses.FirstOrDefault();
        settings.FooterContactPhone = NormalizeOptional(command.FooterContactPhone) ?? footerContactPhones.FirstOrDefault();
        settings.FooterContactPhonesJson = LandingPageSettingsJsonHelper.SerializeStringList(footerContactPhones);
        settings.FooterContactEmail = NormalizeOptional(command.FooterContactEmail);
        settings.FooterAddressesJson = LandingPageSettingsJsonHelper.SerializeStringList(footerAddresses);
        settings.FooterSocialLinksJson = LandingPageSettingsJsonHelper.SerializeFooterSocialLinks(footerSocialLinks);
        settings.FeaturedProgramIdsJson = LandingPageSettingsJsonHelper.SerializeIds(featuredProgramIds);
        settings.FeaturedClassIdsJson = LandingPageSettingsJsonHelper.SerializeIds(featuredClassIds);
        settings.FeaturedProgramConfigsJson = LandingPageSettingsJsonHelper.SerializeFeaturedItemConfigs(featuredProgramConfigs);
        settings.FeaturedClassConfigsJson = LandingPageSettingsJsonHelper.SerializeFeaturedItemConfigs(featuredClassConfigs);
        settings.FeaturedTeacherIdsJson = LandingPageSettingsJsonHelper.SerializeIds(featuredTeacherIds);
        settings.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        var content = await LandingPageReadModelBuilder.BuildAsync(
            context,
            schedulePatternParser,
            settings,
            publicOnly: false,
            cancellationToken);

        return Result.Success(new UpdateLandingPageSettingsResponse
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

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
