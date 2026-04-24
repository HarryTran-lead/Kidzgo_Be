using System.Globalization;
using System.Text.Json;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Services;

internal static class SchedulePatternSupport
{
    private static readonly string[] DayOrder = ["MO", "TU", "WE", "TH", "FR", "SA", "SU"];

    private static readonly IReadOnlyDictionary<string, DayOfWeek> DayCodeToDayOfWeek =
        new Dictionary<string, DayOfWeek>(StringComparer.OrdinalIgnoreCase)
        {
            ["MO"] = DayOfWeek.Monday,
            ["TU"] = DayOfWeek.Tuesday,
            ["WE"] = DayOfWeek.Wednesday,
            ["TH"] = DayOfWeek.Thursday,
            ["FR"] = DayOfWeek.Friday,
            ["SA"] = DayOfWeek.Saturday,
            ["SU"] = DayOfWeek.Sunday
        };

    private static readonly IReadOnlyDictionary<DayOfWeek, string> DayOfWeekToDayCode =
        DayCodeToDayOfWeek.ToDictionary(pair => pair.Value, pair => pair.Key);

    private static readonly IReadOnlyDictionary<string, string> DayLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["MO"] = "Thu 2",
            ["TU"] = "Thu 3",
            ["WE"] = "Thu 4",
            ["TH"] = "Thu 5",
            ["FR"] = "Thu 6",
            ["SA"] = "Thu 7",
            ["SU"] = "Chu nhat"
        };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static Result<string?> NormalizePattern(
        string? schedulePattern,
        IReadOnlyCollection<ScheduleSlot>? weeklyScheduleSlots,
        bool requireValue)
    {
        var hasPattern = !string.IsNullOrWhiteSpace(schedulePattern);
        var hasSlots = weeklyScheduleSlots is { Count: > 0 };

        if (hasPattern && hasSlots)
        {
            return Result.Failure<string?>(Error.Validation(
                "SchedulePattern.Ambiguous",
                "Provide either schedulePattern or weeklyScheduleSlots, not both."));
        }

        if (!hasPattern && !hasSlots)
        {
            return requireValue
                ? Result.Failure<string?>(Error.Validation(
                    "SchedulePattern.Empty",
                    "Schedule pattern cannot be empty"))
                : Result.Success<string?>(null);
        }

        if (hasSlots)
        {
            var slotsResult = NormalizeSlots(weeklyScheduleSlots!);
            if (slotsResult.IsFailure)
            {
                return Result.Failure<string?>(slotsResult.Error);
            }

            return Result.Success<string?>(SerializeStructuredPattern(slotsResult.Value));
        }

        return Result.Success<string?>(schedulePattern!.Trim());
    }

    public static Result<string?> NormalizeWeeklyScheduleJson(
        IReadOnlyCollection<ScheduleSlot>? weeklyScheduleSlots,
        bool requireValue)
    {
        if (weeklyScheduleSlots is not { Count: > 0 })
        {
            return requireValue
                ? Result.Failure<string?>(Error.Validation(
                    "WeeklySchedule.Empty",
                    "Weekly schedule cannot be empty"))
                : Result.Success<string?>(null);
        }

        var slotsResult = NormalizeSlots(weeklyScheduleSlots);
        if (slotsResult.IsFailure)
        {
            return Result.Failure<string?>(slotsResult.Error);
        }

        return Result.Success<string?>(SerializeStructuredPattern(slotsResult.Value));
    }

    public static Result<string?> NormalizeWeeklyPatternJson(
        IReadOnlyCollection<WeeklyPatternEntry>? weeklyPattern,
        bool requireValue)
    {
        if (weeklyPattern is not { Count: > 0 })
        {
            return requireValue
                ? Result.Failure<string?>(Error.Validation(
                    "WeeklyPattern.Empty",
                    "Weekly pattern cannot be empty"))
                : Result.Success<string?>(null);
        }

        var patternResult = NormalizeWeeklyPatternEntries(weeklyPattern);
        if (patternResult.IsFailure)
        {
            return Result.Failure<string?>(patternResult.Error);
        }

        return Result.Success<string?>(SerializeWeeklyPattern(patternResult.Value));
    }

    public static Result<List<ScheduleSlot>> ParseScheduleSlots(string schedulePattern)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                "SchedulePattern.Empty",
                "Schedule pattern cannot be empty"));
        }

        var trimmed = schedulePattern.Trim();
        return IsStructuredPattern(trimmed)
            ? ParseStructuredScheduleSlots(trimmed)
            : ParseLegacyRRuleSlots(trimmed);
    }

    public static string? BuildDisplayText(string? schedulePattern)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return schedulePattern;
        }

        var slotResult = ParseScheduleSlots(schedulePattern);
        if (slotResult.IsFailure)
        {
            return schedulePattern;
        }

        return string.Join(
            ", ",
            slotResult.Value.Select(BuildSlotDisplayText));
    }

    public static Result<List<WeeklyPatternEntry>> ParseWeeklyPattern(string schedulePattern)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return Result.Failure<List<WeeklyPatternEntry>>(Error.Validation(
                "WeeklyPattern.Empty",
                "Weekly pattern cannot be empty"));
        }

        var slotResult = ParseScheduleSlots(schedulePattern);
        if (slotResult.IsFailure)
        {
            return Result.Failure<List<WeeklyPatternEntry>>(slotResult.Error);
        }

        return Result.Success(GroupSlotsToWeeklyPattern(slotResult.Value));
    }

    public static string? BuildDisplayText(IReadOnlyCollection<ScheduleSlot>? slots)
    {
        if (slots is not { Count: > 0 })
        {
            return null;
        }

        return string.Join(", ", slots.Select(BuildSlotDisplayText));
    }

    public static IReadOnlyList<string> ExtractOrderedDayCodes(string? schedulePattern)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return [];
        }

        var slotResult = ParseScheduleSlots(schedulePattern);
        if (slotResult.IsFailure)
        {
            return [];
        }

        var daySet = slotResult.Value
            .Select(slot => slot.DayOfWeek)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return DayOrder
            .Where(daySet.Contains)
            .ToList();
    }

    public static string GetDayCode(DateOnly date)
    {
        return DayOfWeekToDayCode[date.DayOfWeek];
    }

    public static string? ResolveEffectiveWeeklyScheduleJson(
        string? defaultWeeklyScheduleJson,
        IEnumerable<WeeklyScheduleSegmentWindow> segments,
        DateOnly referenceDate)
    {
        var matchingSegment = segments
            .Where(segment => segment.EffectiveFrom <= referenceDate &&
                              (!segment.EffectiveTo.HasValue || referenceDate <= segment.EffectiveTo.Value))
            .OrderByDescending(segment => segment.EffectiveFrom)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(matchingSegment.WeeklyScheduleJson)
            ? defaultWeeklyScheduleJson
            : matchingSegment.WeeklyScheduleJson;
    }

    public static DayOfWeek ToDayOfWeek(string dayCode)
    {
        return DayCodeToDayOfWeek[dayCode];
    }

    public static bool IsStructuredPattern(string schedulePattern)
    {
        var trimmed = schedulePattern.TrimStart();
        return trimmed.StartsWith("{", StringComparison.Ordinal) ||
               trimmed.StartsWith("[", StringComparison.Ordinal);
    }

    private static Result<List<ScheduleSlot>> NormalizeSlots(IEnumerable<ScheduleSlot> slots)
    {
        var normalizedSlots = new List<ScheduleSlot>();

        foreach (var slot in slots)
        {
            var dayCode = slot.DayOfWeek?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(dayCode) || !DayCodeToDayOfWeek.ContainsKey(dayCode))
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.InvalidDayOfWeek",
                    $"Invalid weekly schedule dayOfWeek: '{slot.DayOfWeek}'. Use MO, TU, WE, TH, FR, SA, or SU."));
            }

            if (!TimeOnly.TryParseExact(
                    slot.StartTime?.Trim(),
                    ["HH:mm", "H:mm", "HH:mm:ss"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var startTime))
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.InvalidStartTime",
                    $"Invalid weekly schedule startTime: '{slot.StartTime}'. Use HH:mm format."));
            }

            if (slot.DurationMinutes <= 0)
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.InvalidDuration",
                    "Weekly schedule durationMinutes must be greater than 0."));
            }

            normalizedSlots.Add(new ScheduleSlot
            {
                DayOfWeek = dayCode,
                StartTime = startTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                DurationMinutes = slot.DurationMinutes
            });
        }

        var duplicateSlot = normalizedSlots
            .GroupBy(slot => $"{slot.DayOfWeek}|{slot.StartTime}", StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateSlot is not null)
        {
            return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                "SchedulePattern.DuplicateSlot",
                $"Duplicate weekly schedule slot found for {duplicateSlot.First().DayOfWeek} at {duplicateSlot.First().StartTime}."));
        }

        return Result.Success(
            normalizedSlots
                .OrderBy(slot => Array.IndexOf(DayOrder, slot.DayOfWeek))
                .ThenBy(slot => slot.StartTime, StringComparer.Ordinal)
                .ToList());
    }

    private static string SerializeStructuredPattern(IReadOnlyCollection<ScheduleSlot> slots)
    {
        var payload = new StructuredSchedulePatternPayload
        {
            Type = "weekly-slots",
            Slots = slots.ToList()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string SerializeWeeklyPattern(IReadOnlyCollection<WeeklyPatternEntry> entries)
    {
        var payload = new WeeklyPatternPayload
        {
            Type = "weekly-pattern",
            Entries = entries.ToList()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string BuildSlotDisplayText(ScheduleSlot slot)
    {
        var label = DayLabels.TryGetValue(slot.DayOfWeek, out var dayLabel)
            ? dayLabel
            : slot.DayOfWeek;

        if (!TimeOnly.TryParseExact(
                slot.StartTime,
                ["HH:mm", "H:mm", "HH:mm:ss"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var startTime))
        {
            return $"{label} {slot.StartTime}";
        }

        var endTime = startTime.AddMinutes(slot.DurationMinutes);
        return $"{label} {startTime:HH:mm}-{endTime:HH:mm}";
    }

    private static Result<List<ScheduleSlot>> ParseStructuredScheduleSlots(string schedulePattern)
    {
        try
        {
            if (schedulePattern.TrimStart().StartsWith("[", StringComparison.Ordinal))
            {
                var arraySlots = JsonSerializer.Deserialize<List<ScheduleSlot>>(schedulePattern, JsonOptions);
                if (arraySlots is null)
                {
                    return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                        "SchedulePattern.Invalid",
                        "Structured schedule pattern could not be parsed."));
                }

                return NormalizeSlots(arraySlots);
            }

            var payload = JsonSerializer.Deserialize<StructuredSchedulePatternPayload>(schedulePattern, JsonOptions);
            if (payload?.Slots is { Count: > 0 })
            {
                return NormalizeSlots(payload.Slots);
            }

            var weeklyPatternPayload = JsonSerializer.Deserialize<WeeklyPatternPayload>(schedulePattern, JsonOptions);
            if (weeklyPatternPayload?.Entries is { Count: > 0 })
            {
                var weeklyPatternResult = NormalizeWeeklyPatternEntries(weeklyPatternPayload.Entries);
                if (weeklyPatternResult.IsFailure)
                {
                    return Result.Failure<List<ScheduleSlot>>(weeklyPatternResult.Error);
                }

                return Result.Success(FlattenWeeklyPatternEntries(weeklyPatternResult.Value));
            }

            if (payload?.Slots is null || payload.Slots.Count == 0)
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.Invalid",
                    "Structured schedule pattern must contain at least one slot."));
            }

            return NormalizeSlots(payload.Slots);
        }
        catch (JsonException ex)
        {
            return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                "SchedulePattern.Invalid",
                $"Invalid structured schedule pattern: {ex.Message}"));
        }
    }

    private static Result<List<WeeklyPatternEntry>> NormalizeWeeklyPatternEntries(
        IEnumerable<WeeklyPatternEntry> entries)
    {
        var normalizedEntries = new List<WeeklyPatternEntry>();

        foreach (var entry in entries)
        {
            if (entry.DayOfWeeks is not { Count: > 0 })
            {
                return Result.Failure<List<WeeklyPatternEntry>>(Error.Validation(
                    "WeeklyPattern.EmptyDayOfWeeks",
                    "Each weeklyPattern entry must contain at least one dayOfWeeks value."));
            }

            if (!TimeOnly.TryParseExact(
                    entry.StartTime?.Trim(),
                    ["HH:mm", "H:mm", "HH:mm:ss"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var startTime))
            {
                return Result.Failure<List<WeeklyPatternEntry>>(Error.Validation(
                    "WeeklyPattern.InvalidStartTime",
                    $"Invalid weeklyPattern startTime: '{entry.StartTime}'. Use HH:mm format."));
            }

            if (entry.DurationMinutes <= 0)
            {
                return Result.Failure<List<WeeklyPatternEntry>>(Error.Validation(
                    "WeeklyPattern.InvalidDuration",
                    "weeklyPattern durationMinutes must be greater than 0."));
            }

            var normalizedDays = entry.DayOfWeeks
                .Select(day => day?.Trim().ToUpperInvariant())
                .ToList();

            if (normalizedDays.Any(day => string.IsNullOrWhiteSpace(day) || !DayCodeToDayOfWeek.ContainsKey(day)))
            {
                return Result.Failure<List<WeeklyPatternEntry>>(Error.Validation(
                    "WeeklyPattern.InvalidDayOfWeek",
                    "weeklyPattern dayOfWeeks must use MO, TU, WE, TH, FR, SA, or SU."));
            }

            var distinctDays = normalizedDays
                .Where(day => !string.IsNullOrWhiteSpace(day))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(day => Array.IndexOf(DayOrder, day))
                .ToList();

            normalizedEntries.Add(new WeeklyPatternEntry
            {
                DayOfWeeks = distinctDays,
                StartTime = startTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                DurationMinutes = entry.DurationMinutes
            });
        }

        var duplicateSlot = normalizedEntries
            .SelectMany(entry => entry.DayOfWeeks.Select(day => $"{day}|{entry.StartTime}"))
            .GroupBy(key => key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateSlot is not null)
        {
            var duplicateParts = duplicateSlot.Key.Split('|', 2);
            return Result.Failure<List<WeeklyPatternEntry>>(Error.Validation(
                "WeeklyPattern.DuplicateSlot",
                $"Duplicate weeklyPattern slot found for {duplicateParts[0]} at {duplicateParts[1]}."));
        }

        return Result.Success(
            normalizedEntries
                .OrderBy(entry => entry.DayOfWeeks.Min(day => Array.IndexOf(DayOrder, day)))
                .ThenBy(entry => entry.StartTime, StringComparer.Ordinal)
                .ToList());
    }

    private static List<ScheduleSlot> FlattenWeeklyPatternEntries(
        IEnumerable<WeeklyPatternEntry> entries)
    {
        return entries
            .SelectMany(entry => entry.DayOfWeeks.Select(day => new ScheduleSlot
            {
                DayOfWeek = day,
                StartTime = entry.StartTime,
                DurationMinutes = entry.DurationMinutes
            }))
            .OrderBy(slot => Array.IndexOf(DayOrder, slot.DayOfWeek))
            .ThenBy(slot => slot.StartTime, StringComparer.Ordinal)
            .ToList();
    }

    private static List<WeeklyPatternEntry> GroupSlotsToWeeklyPattern(IEnumerable<ScheduleSlot> slots)
    {
        return slots
            .GroupBy(slot => $"{slot.StartTime}|{slot.DurationMinutes}", StringComparer.Ordinal)
            .OrderBy(group => group.Min(slot => Array.IndexOf(DayOrder, slot.DayOfWeek)))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var parts = group.Key.Split('|', 2);
                return new WeeklyPatternEntry
                {
                    StartTime = parts[0],
                    DurationMinutes = int.TryParse(parts[1], out var d) ? d : 0,
                    DayOfWeeks = group
                        .Select(slot => slot.DayOfWeek)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(day => Array.IndexOf(DayOrder, day))
                        .ToList()
                };
            })
            .ToList();
    }

    private static Result<List<ScheduleSlot>> ParseLegacyRRuleSlots(string schedulePattern)
    {
        try
        {
            var pattern = schedulePattern.Trim();
            if (pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern[6..];
            }

            var parameters = pattern
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(parameter => parameter.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim().ToUpperInvariant(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);

            if (!parameters.TryGetValue("BYDAY", out var dayValue))
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.Invalid",
                    "RRULE schedule pattern must contain BYDAY."));
            }

            if (!parameters.TryGetValue("BYHOUR", out var hourValue) ||
                !int.TryParse(hourValue.Split(',')[0], out var hour))
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.Invalid",
                    "RRULE schedule pattern must contain a valid BYHOUR."));
            }

            var minute = 0;
            if (parameters.TryGetValue("BYMINUTE", out var minuteValue))
            {
                int.TryParse(minuteValue.Split(',')[0], out minute);
            }

            if (hour is < 0 or > 23 || minute is < 0 or > 59)
            {
                return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                    "SchedulePattern.Invalid",
                    "RRULE schedule pattern contains an invalid BYHOUR or BYMINUTE."));
            }

            var duration = 90;
            if (parameters.TryGetValue("DURATION", out var durationValue) &&
                int.TryParse(durationValue, out var parsedDuration) &&
                parsedDuration > 0)
            {
                duration = parsedDuration;
            }

            var slots = dayValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(dayCode => new ScheduleSlot
                {
                    DayOfWeek = dayCode.Trim().ToUpperInvariant(),
                    StartTime = $"{hour:00}:{minute:00}",
                    DurationMinutes = duration
                })
                .ToList();

            return NormalizeSlots(slots);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ScheduleSlot>>(Error.Validation(
                "SchedulePattern.Invalid",
                $"Invalid RRULE schedule pattern: {ex.Message}"));
        }
    }

    private sealed class StructuredSchedulePatternPayload
    {
        public string Type { get; set; } = "weekly-slots";
        public List<ScheduleSlot> Slots { get; set; } = [];
    }

    private sealed class WeeklyPatternPayload
    {
        public string Type { get; set; } = "weekly-pattern";
        public List<WeeklyPatternEntry> Entries { get; set; } = [];
    }
}
