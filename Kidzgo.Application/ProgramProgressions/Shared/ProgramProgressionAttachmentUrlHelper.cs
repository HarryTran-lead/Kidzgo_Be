using System.Text.Json;

namespace Kidzgo.Application.ProgramProgressions.Shared;

internal static class ProgramProgressionAttachmentUrlHelper
{
    public static string? Serialize(IReadOnlyCollection<string>? attachmentUrls)
    {
        if (attachmentUrls is null || attachmentUrls.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            attachmentUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct()
                .ToList());
    }

    public static IReadOnlyList<string> Parse(string? attachmentUrlsJson)
    {
        if (string.IsNullOrWhiteSpace(attachmentUrlsJson))
        {
            return Array.Empty<string>();
        }

        try
        {
            if (attachmentUrlsJson.TrimStart().StartsWith("[", StringComparison.Ordinal))
            {
                var urls = JsonSerializer.Deserialize<List<string>>(attachmentUrlsJson);

                if (urls is null)
                {
                    return Array.Empty<string>();
                }

                return urls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .Distinct()
                    .ToList();
            }

            return new[] { attachmentUrlsJson.Trim() };
        }
        catch
        {
            return new[] { attachmentUrlsJson.Trim() };
        }
    }
}
