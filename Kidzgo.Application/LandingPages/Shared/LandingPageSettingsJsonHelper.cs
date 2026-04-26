using System.Text.Json;

namespace Kidzgo.Application.LandingPages.Shared;

internal static class LandingPageSettingsJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static IReadOnlyList<Guid> DeserializeIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<Guid>();
        }

        try
        {
            var ids = JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions) ?? [];
            var seen = new HashSet<Guid>();
            var orderedIds = new List<Guid>();

            foreach (var id in ids)
            {
                if (id == Guid.Empty || !seen.Add(id))
                {
                    continue;
                }

                orderedIds.Add(id);
            }

            return orderedIds;
        }
        catch (JsonException)
        {
            return Array.Empty<Guid>();
        }
    }

    public static string SerializeIds(IEnumerable<Guid>? ids)
    {
        if (ids is null)
        {
            return "[]";
        }

        var seen = new HashSet<Guid>();
        var orderedIds = new List<Guid>();

        foreach (var id in ids)
        {
            if (id == Guid.Empty || !seen.Add(id))
            {
                continue;
            }

            orderedIds.Add(id);
        }

        return JsonSerializer.Serialize(orderedIds, JsonOptions);
    }

    public static IReadOnlyList<LandingPageFeaturedItemConfigDto> DeserializeFeaturedItemConfigs(
        string? json,
        string? fallbackIdsJson = null)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<FeaturedItemConfigModel>>(json, JsonOptions) ?? [];
                return NormalizeFeaturedItemConfigs(items.Select(item => new LandingPageFeaturedItemConfigDto
                {
                    Id = item.Id,
                    Tags = item.Tags ?? []
                }));
            }
            catch (JsonException)
            {
            }
        }

        return DeserializeIds(fallbackIdsJson)
            .Select(id => new LandingPageFeaturedItemConfigDto
            {
                Id = id
            })
            .ToList();
    }

    public static string SerializeFeaturedItemConfigs(IEnumerable<LandingPageFeaturedItemConfigDto>? items)
    {
        var normalizedItems = NormalizeFeaturedItemConfigs(items);
        var models = normalizedItems
            .Select(item => new FeaturedItemConfigModel
            {
                Id = item.Id,
                Tags = item.Tags.ToList()
            })
            .ToList();

        return JsonSerializer.Serialize(models, JsonOptions);
    }

    public static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
            return NormalizeStringList(items);
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    public static string SerializeStringList(IEnumerable<string>? items)
    {
        var normalizedItems = NormalizeStringList(items);
        return JsonSerializer.Serialize(normalizedItems, JsonOptions);
    }

    public static IReadOnlyList<LandingPageFooterSocialLinkDto> DeserializeFooterSocialLinks(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<LandingPageFooterSocialLinkDto>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<FooterSocialLinkModel>>(json, JsonOptions) ?? [];
            return NormalizeFooterSocialLinks(items.Select(item => new LandingPageFooterSocialLinkDto
            {
                Label = item.Label ?? string.Empty,
                Url = item.Url ?? string.Empty,
                IconKey = item.IconKey
            }));
        }
        catch (JsonException)
        {
            return Array.Empty<LandingPageFooterSocialLinkDto>();
        }
    }

    public static string SerializeFooterSocialLinks(IEnumerable<LandingPageFooterSocialLinkDto>? items)
    {
        var normalizedItems = NormalizeFooterSocialLinks(items);
        var models = normalizedItems
            .Select(item => new FooterSocialLinkModel
            {
                Label = item.Label,
                Url = item.Url,
                IconKey = item.IconKey
            })
            .ToList();

        return JsonSerializer.Serialize(models, JsonOptions);
    }

    private static IReadOnlyList<LandingPageFeaturedItemConfigDto> NormalizeFeaturedItemConfigs(
        IEnumerable<LandingPageFeaturedItemConfigDto>? items)
    {
        if (items is null)
        {
            return Array.Empty<LandingPageFeaturedItemConfigDto>();
        }

        var seenIds = new HashSet<Guid>();
        var normalizedItems = new List<LandingPageFeaturedItemConfigDto>();

        foreach (var item in items)
        {
            if (item.Id == Guid.Empty || !seenIds.Add(item.Id))
            {
                continue;
            }

            normalizedItems.Add(new LandingPageFeaturedItemConfigDto
            {
                Id = item.Id,
                Tags = NormalizeStringList(item.Tags)
            });
        }

        return normalizedItems;
    }

    private static IReadOnlyList<string> NormalizeStringList(IEnumerable<string>? items)
    {
        if (items is null)
        {
            return Array.Empty<string>();
        }

        var seenItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedItems = new List<string>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            var normalizedItem = item.Trim();
            if (!seenItems.Add(normalizedItem))
            {
                continue;
            }

            normalizedItems.Add(normalizedItem);
        }

        return normalizedItems;
    }

    private static IReadOnlyList<LandingPageFooterSocialLinkDto> NormalizeFooterSocialLinks(
        IEnumerable<LandingPageFooterSocialLinkDto>? items)
    {
        if (items is null)
        {
            return Array.Empty<LandingPageFooterSocialLinkDto>();
        }

        var seenLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedItems = new List<LandingPageFooterSocialLinkDto>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Label) || string.IsNullOrWhiteSpace(item.Url))
            {
                continue;
            }

            var normalizedLabel = item.Label.Trim();
            var normalizedUrl = item.Url.Trim();
            var normalizedIconKey = NormalizeOptional(item.IconKey);
            var uniqueKey = $"{normalizedLabel}|{normalizedUrl}";

            if (!seenLinks.Add(uniqueKey))
            {
                continue;
            }

            normalizedItems.Add(new LandingPageFooterSocialLinkDto
            {
                Label = normalizedLabel,
                Url = normalizedUrl,
                IconKey = normalizedIconKey
            });
        }

        return normalizedItems;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private sealed class FeaturedItemConfigModel
    {
        public Guid Id { get; set; }
        public List<string>? Tags { get; set; }
    }

    private sealed class FooterSocialLinkModel
    {
        public string? Label { get; set; }
        public string? Url { get; set; }
        public string? IconKey { get; set; }
    }
}
