using FluentValidation;

namespace Kidzgo.Application.LandingPages.UpdateLandingPageSettings;

public sealed class UpdateLandingPageSettingsCommandValidator : AbstractValidator<UpdateLandingPageSettingsCommand>
{
    public UpdateLandingPageSettingsCommandValidator()
    {
        RuleFor(command => command.LogoUrl)
            .MaximumLength(1000).WithMessage("Logo URL must not exceed 1000 characters.");

        RuleFor(command => command.FeaturedProgramsSectionTitle)
            .MaximumLength(255).WithMessage("Featured programs section title must not exceed 255 characters.");

        RuleFor(command => command.FeaturedProgramsSectionSubtitle)
            .MaximumLength(1000).WithMessage("Featured programs section subtitle must not exceed 1000 characters.");

        RuleFor(command => command.FeaturedClassesSectionTitle)
            .MaximumLength(255).WithMessage("Featured classes section title must not exceed 255 characters.");

        RuleFor(command => command.FeaturedClassesSectionSubtitle)
            .MaximumLength(1000).WithMessage("Featured classes section subtitle must not exceed 1000 characters.");

        RuleFor(command => command.FeaturedTeachersSectionTitle)
            .MaximumLength(255).WithMessage("Featured teachers section title must not exceed 255 characters.");

        RuleFor(command => command.FeaturedTeachersSectionSubtitle)
            .MaximumLength(1000).WithMessage("Featured teachers section subtitle must not exceed 1000 characters.");

        RuleFor(command => command.FooterAddress)
            .MaximumLength(500).WithMessage("Footer address must not exceed 500 characters.");

        RuleFor(command => command.FooterContactPhone)
            .MaximumLength(100).WithMessage("Footer contact phone must not exceed 100 characters.");

        RuleFor(command => command.FooterContactPhones)
            .NotNull().WithMessage("Footer contact phones are required.")
            .Must(HaveValidFooterContactPhones)
            .WithMessage("Footer contact phones must be non-empty and no more than 100 characters each.");

        RuleFor(command => command.FooterContactEmail)
            .MaximumLength(255).WithMessage("Footer contact email must not exceed 255 characters.");

        RuleFor(command => command.FooterAddresses)
            .NotNull().WithMessage("Footer addresses are required.")
            .Must(HaveValidFooterAddresses)
            .WithMessage("Footer addresses must be non-empty and no more than 500 characters each.");

        RuleForEach(command => command.FooterSocialLinks)
            .ChildRules(link =>
            {
                link.RuleFor(item => item.Label)
                    .NotEmpty().WithMessage("Footer social link label is required.")
                    .MaximumLength(100).WithMessage("Footer social link label must not exceed 100 characters.");

                link.RuleFor(item => item.Url)
                    .NotEmpty().WithMessage("Footer social link URL is required.")
                    .MaximumLength(1000).WithMessage("Footer social link URL must not exceed 1000 characters.");

                link.RuleFor(item => item.IconKey)
                    .MaximumLength(100).WithMessage("Footer social link icon key must not exceed 100 characters.");
            });

        RuleFor(command => command.FeaturedPrograms)
            .Must(HaveUniqueNonEmptyFeaturedItemIds)
            .WithMessage("Featured programs must be unique and non-empty.");

        RuleForEach(command => command.FeaturedPrograms)
            .ChildRules(item =>
            {
                item.RuleFor(config => config.Tags)
                    .Must(HaveValidTags)
                    .WithMessage("Featured program tags must be unique, non-empty, and no more than 100 characters each.");
            });

        RuleFor(command => command.FeaturedClasses)
            .Must(HaveUniqueNonEmptyFeaturedItemIds)
            .WithMessage("Featured classes must be unique and non-empty.");

        RuleForEach(command => command.FeaturedClasses)
            .ChildRules(item =>
            {
                item.RuleFor(config => config.Tags)
                    .Must(HaveValidTags)
                    .WithMessage("Featured class tags must be unique, non-empty, and no more than 100 characters each.");
            });

        RuleFor(command => command.FeaturedTeacherIds)
            .Must(HaveUniqueNonEmptyIds)
            .WithMessage("Featured teacher IDs must be unique and non-empty.");
    }

    private static bool HaveUniqueNonEmptyIds(IReadOnlyList<Guid>? ids)
    {
        if (ids is null)
        {
            return false;
        }

        var seen = new HashSet<Guid>();
        foreach (var id in ids)
        {
            if (id == Guid.Empty || !seen.Add(id))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveUniqueNonEmptyFeaturedItemIds(IReadOnlyList<LandingPageFeaturedItemInput>? items)
    {
        if (items is null)
        {
            return false;
        }

        var seenIds = new HashSet<Guid>();
        foreach (var item in items)
        {
            if (item.Id == Guid.Empty || !seenIds.Add(item.Id))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveValidTags(IReadOnlyList<string>? tags)
    {
        if (tags is null)
        {
            return false;
        }

        var seenTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            var trimmedTag = tag.Trim();
            if (trimmedTag.Length > 100 || !seenTags.Add(trimmedTag))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveValidFooterAddresses(IReadOnlyList<string>? addresses)
    {
        if (addresses is null)
        {
            return false;
        }

        foreach (var address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address) || address.Trim().Length > 500)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveValidFooterContactPhones(IReadOnlyList<string>? phones)
    {
        if (phones is null)
        {
            return false;
        }

        foreach (var phone in phones)
        {
            if (string.IsNullOrWhiteSpace(phone) || phone.Trim().Length > 100)
            {
                return false;
            }
        }

        return true;
    }
}
