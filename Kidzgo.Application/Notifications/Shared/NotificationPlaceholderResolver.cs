using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.Shared;

internal static class NotificationPlaceholderResolver
{
    public static async Task<Dictionary<string, string>> ResolveAsync(
        IDbContext context,
        Notification notification,
        CancellationToken cancellationToken)
    {
        if (string.Equals(notification.Kind, "pause_enrollment", StringComparison.OrdinalIgnoreCase))
        {
            return await ResolvePauseEnrollmentAsync(context, notification, cancellationToken);
        }

        return new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public static bool ContainsPlaceholders(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.Contains("{{", StringComparison.Ordinal) &&
               text.Contains("}}", StringComparison.Ordinal);
    }

    private static async Task<Dictionary<string, string>> ResolvePauseEnrollmentAsync(
        IDbContext context,
        Notification notification,
        CancellationToken cancellationToken)
    {
        var requestId = ExtractPauseEnrollmentRequestId(notification.Deeplink);
        if (!requestId.HasValue)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var data = await (
            from request in context.PauseEnrollmentRequests
            join profile in context.Profiles on request.StudentProfileId equals profile.Id
            where request.Id == requestId.Value
            select new
            {
                profile.DisplayName,
                request.PauseFrom,
                request.PauseTo,
                request.Outcome,
                request.OutcomeNote
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["student_name"] = data.DisplayName ?? "Hoc sinh",
            ["pause_from"] = data.PauseFrom.ToString("dd/MM/yyyy"),
            ["pause_to"] = data.PauseTo.ToString("dd/MM/yyyy"),
            ["outcome"] = VietnameseEnumText.ForPauseEnrollmentOutcome(data.Outcome),
            ["outcome_note"] = data.OutcomeNote ?? string.Empty
        };
    }

    private static Guid? ExtractPauseEnrollmentRequestId(string? deeplink)
    {
        if (string.IsNullOrWhiteSpace(deeplink))
        {
            return null;
        }

        const string segment = "/pause-enrollment-requests/";
        var startIndex = deeplink.IndexOf(segment, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
        {
            return null;
        }

        startIndex += segment.Length;
        var endIndex = deeplink.IndexOfAny(new[] { '/', '?', '#' }, startIndex);
        var rawId = endIndex >= 0
            ? deeplink[startIndex..endIndex]
            : deeplink[startIndex..];

        return Guid.TryParse(rawId, out var requestId) ? requestId : null;
    }
}
