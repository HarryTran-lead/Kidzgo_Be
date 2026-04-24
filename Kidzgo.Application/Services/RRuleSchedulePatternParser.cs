using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Services;

public sealed class RRuleSchedulePatternParser : ISchedulePatternParser
{
    public Result<List<DateTime>> ParseAndGenerateOccurrences(string schedulePattern, DateOnly startDate, DateOnly? endDate)
    {
        var detailedResult = ParseAndGenerateOccurrenceDetails(schedulePattern, startDate, endDate);
        if (detailedResult.IsFailure)
        {
            return Result.Failure<List<DateTime>>(detailedResult.Error);
        }

        return Result.Success(detailedResult.Value
            .Select(occurrence => occurrence.PlannedDatetime)
            .OrderBy(value => value)
            .ToList());
    }

    public Result<List<ScheduleOccurrence>> ParseAndGenerateOccurrenceDetails(
        string schedulePattern,
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return Result.Failure<List<ScheduleOccurrence>>(
                Error.Validation("SchedulePattern.Empty", "Schedule pattern cannot be empty"));
        }

        var trimmedPattern = schedulePattern.Trim();

        return SchedulePatternSupport.IsStructuredPattern(trimmedPattern)
            ? ParseStructuredSchedule(trimmedPattern, startDate, endDate)
            : ParseLegacyRRule(trimmedPattern, startDate, endDate);
    }

    public int? ParseDuration(string schedulePattern)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return null;
        }

        if (SchedulePatternSupport.IsStructuredPattern(schedulePattern))
        {
            var slotsResult = ParseScheduleSlots(schedulePattern);
            if (slotsResult.IsFailure || slotsResult.Value.Count == 0)
            {
                return null;
            }

            var durations = slotsResult.Value
                .Select(slot => slot.DurationMinutes)
                .Distinct()
                .ToList();

            return durations.Count == 1 ? durations[0] : null;
        }

        try
        {
            var pattern = schedulePattern.Trim();
            if (pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern[6..];
            }

            var parameters = pattern.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in parameters)
            {
                var parts = param.Split('=', 2);
                if (parts.Length == 2 &&
                    parts[0].Trim().Equals("DURATION", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[1].Trim(), out var duration) &&
                    duration > 0)
                {
                    return duration;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public Result<List<ScheduleSlot>> ParseScheduleSlots(string schedulePattern)
    {
        return SchedulePatternSupport.ParseScheduleSlots(schedulePattern);
    }

    private static Result<List<ScheduleOccurrence>> ParseStructuredSchedule(
        string schedulePattern,
        DateOnly startDate,
        DateOnly? endDate)
    {
        var slotsResult = SchedulePatternSupport.ParseScheduleSlots(schedulePattern);
        if (slotsResult.IsFailure)
        {
            return Result.Failure<List<ScheduleOccurrence>>(slotsResult.Error);
        }

        var effectiveEndDate = endDate ?? startDate;
        if (effectiveEndDate < startDate)
        {
            return Result.Success(new List<ScheduleOccurrence>());
        }

        var occurrences = new List<ScheduleOccurrence>();
        for (var currentDate = startDate; currentDate <= effectiveEndDate; currentDate = currentDate.AddDays(1))
        {
            var dayCode = SchedulePatternSupport.GetDayCode(currentDate);

            foreach (var slot in slotsResult.Value.Where(slot =>
                         slot.DayOfWeek.Equals(dayCode, StringComparison.OrdinalIgnoreCase)))
            {
                if (!TimeOnly.TryParseExact(
                        slot.StartTime,
                        ["HH:mm", "H:mm", "HH:mm:ss"],
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var startTime))
                {
                    return Result.Failure<List<ScheduleOccurrence>>(Error.Validation(
                        "SchedulePattern.InvalidStartTime",
                        $"Invalid structured schedule startTime: '{slot.StartTime}'."));
                }

                var utc = ConvertVietnamLocalDateTimeToUtc(currentDate, startTime);
                occurrences.Add(new ScheduleOccurrence
                {
                    PlannedDatetime = utc,
                    DurationMinutes = slot.DurationMinutes
                });
            }
        }

        return Result.Success(occurrences
            .OrderBy(occurrence => occurrence.PlannedDatetime)
            .ToList());
    }

    private static Result<List<ScheduleOccurrence>> ParseLegacyRRule(
        string schedulePattern,
        DateOnly startDate,
        DateOnly? endDate)
    {
        try
        {
            var pattern = schedulePattern.Trim();
            if (!pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = "RRULE:" + pattern;
            }

            var patternWithoutDuration = RemoveDurationParameter(pattern);
            var recurrencePattern = new RecurrencePattern(patternWithoutDuration);
            var durationMinutes = ParseLegacyDuration(schedulePattern) ?? 90;

            var startTimeOnly = TimeOnly.MinValue;
            if (recurrencePattern.ByHour.Count > 0 && recurrencePattern.ByMinute.Count > 0)
            {
                startTimeOnly = new TimeOnly(
                    recurrencePattern.ByHour.First(),
                    recurrencePattern.ByMinute.First());
            }

            var startDateTime = ConvertVietnamLocalDateTimeToUtc(startDate, startTimeOnly);
            var calendarEvent = new CalendarEvent
            {
                Start = new CalDateTime(startDateTime),
                RecurrenceRules = new List<RecurrencePattern> { recurrencePattern }
            };

            if (endDate.HasValue)
            {
                calendarEvent.RecurrenceRules[0].Until =
                    ConvertVietnamLocalDateTimeToUtc(endDate.Value, TimeOnly.MaxValue);
            }

            var until = endDate.HasValue
                ? ConvertVietnamLocalDateTimeToUtc(endDate.Value, TimeOnly.MaxValue)
                : DateTime.MaxValue;

            var occurrences = calendarEvent.GetOccurrences(startDateTime, until)
                .Select(occurrence => occurrence.Period.StartTime.Value.ToUniversalTime())
                .Where(value => value >= startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                .Where(value => !endDate.HasValue || DateOnly.FromDateTime(value.Date) <= endDate.Value)
                .Distinct()
                .OrderBy(value => value)
                .Select(value => new ScheduleOccurrence
                {
                    PlannedDatetime = value,
                    DurationMinutes = durationMinutes
                })
                .ToList();

            return Result.Success(occurrences);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ScheduleOccurrence>>(
                Error.Validation("SchedulePattern.Invalid", $"Invalid schedule pattern: {ex.Message}"));
        }
    }

    private static int? ParseLegacyDuration(string schedulePattern)
    {
        try
        {
            var pattern = schedulePattern.Trim();
            if (pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern[6..];
            }

            return pattern
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(parameter => parameter.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .Where(parts => parts[0].Trim().Equals("DURATION", StringComparison.OrdinalIgnoreCase))
                .Select(parts => int.TryParse(parts[1].Trim(), out var duration) ? duration : (int?)null)
                .FirstOrDefault(duration => duration.HasValue && duration.Value > 0);
        }
        catch
        {
            return null;
        }
    }

    private static string RemoveDurationParameter(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return pattern;
        }

        var hasPrefix = pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase);
        var patternWithoutPrefix = hasPrefix ? pattern[6..] : pattern;
        var parameters = patternWithoutPrefix
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(parameter => !parameter.Trim().StartsWith("DURATION=", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var cleanedPattern = string.Join(";", parameters);
        if (hasPrefix)
        {
            cleanedPattern = "RRULE:" + cleanedPattern;
        }

        return cleanedPattern;
    }

    private static DateTime ConvertVietnamLocalDateTimeToUtc(DateOnly date, TimeOnly time)
    {
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, vnTimeZone);
    }
}
