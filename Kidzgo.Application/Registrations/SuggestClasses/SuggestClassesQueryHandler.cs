using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kidzgo.Application.Registrations.SuggestClasses.Handler;

public sealed class SuggestClassesQueryHandler(
    IDbContext context,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<SuggestClassesQuery, SuggestClassesResponse>
{
    public async Task<Result<SuggestClassesResponse>> Handle(
        SuggestClassesQuery query,
        CancellationToken cancellationToken)
    {
        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.SecondaryProgram)
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == query.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<SuggestClassesResponse>(RegistrationErrors.NotFound(query.RegistrationId));
        }

        var primarySuggestions = await BuildSuggestionsAsync(
            registration.ProgramId,
            registration.BranchId,
            registration.PreferredSchedule,
            cancellationToken);

        var secondarySuggestions = registration.SecondaryProgramId.HasValue
            ? await BuildSuggestionsAsync(
                registration.SecondaryProgramId.Value,
                registration.BranchId,
                registration.PreferredSchedule,
                cancellationToken)
            : (Suggested: new List<SuggestedClassDto>(), Alternative: new List<SuggestedClassDto>());

        return new SuggestClassesResponse
        {
            RegistrationId = registration.Id,
            ProgramName = registration.Program.Name,
            BranchName = registration.Branch.Name,
            PreferredSchedule = registration.PreferredSchedule,
            SuggestedClasses = primarySuggestions.Suggested,
            AlternativeClasses = primarySuggestions.Alternative,
            SecondaryProgramId = registration.SecondaryProgramId,
            SecondaryProgramName = registration.SecondaryProgram?.Name,
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            SecondarySuggestedClasses = secondarySuggestions.Suggested,
            SecondaryAlternativeClasses = secondarySuggestions.Alternative
        };
    }

    private async Task<(List<SuggestedClassDto> Suggested, List<SuggestedClassDto> Alternative)> BuildSuggestionsAsync(
        Guid programId,
        Guid branchId,
        string? preferredSchedule,
        CancellationToken cancellationToken)
    {
        var matchingClasses = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.ClassEnrollments)
            .Include(c => c.ScheduleSegments)
            .Where(c => c.ProgramId == programId
                && c.BranchId == branchId
                && (c.Status == ClassStatus.Recruiting || c.Status == ClassStatus.Active || c.Status == ClassStatus.Planned || c.Status == ClassStatus.Full)
                && c.Capacity > c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active))
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);

        var today = VietnamTime.TodayDateOnly();

        var filteredClasses = matchingClasses
            .Where(c => IsScheduleMatching(
                preferredSchedule ?? string.Empty,
                ResolveEffectiveWeeklyScheduleJson(c, today),
                schedulePatternParser))
            .ToList();

        return (
            filteredClasses
                .Where(c => c.StartDate <= today.AddDays(7))
                .Select(c => MapSuggestedClass(c, today))
                .ToList(),
            filteredClasses
                .Where(c => c.StartDate > today.AddDays(7))
                .Select(c => MapSuggestedClass(c, today))
                .ToList());
    }

    private static SuggestedClassDto MapSuggestedClass(Kidzgo.Domain.Classes.Class classEntity, DateOnly now)
    {
        var currentEnrollment = classEntity.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active);
        var status = classEntity.Status == ClassStatus.Full && currentEnrollment < classEntity.Capacity
            ? (classEntity.StartDate <= now ? ClassStatus.Active : ClassStatus.Recruiting)
            : classEntity.Status;

        return new SuggestedClassDto
        {
            Id = classEntity.Id,
            Code = classEntity.Code,
            Title = classEntity.Title,
            Status = status.ToString(),
            Capacity = classEntity.Capacity,
            CurrentEnrollment = currentEnrollment,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            WeeklyScheduleSlots = ParseWeeklyScheduleSlots(ResolveEffectiveWeeklyScheduleJson(classEntity, now)),
            MainTeacherName = classEntity.MainTeacher != null ? classEntity.MainTeacher.Name : "Not assigned",
            ClassroomName = null,
            IsClassStarted = classEntity.StartDate <= now
        };
    }

    private static List<ScheduleSlot> ParseWeeklyScheduleSlots(string? weeklyScheduleJson)
    {
        if (string.IsNullOrWhiteSpace(weeklyScheduleJson))
        {
            return [];
        }

        var parseResult = SchedulePatternSupport.ParseScheduleSlots(weeklyScheduleJson);
        return parseResult.IsSuccess ? parseResult.Value : [];
    }

    private static bool IsScheduleMatching(
        string preferredSchedule,
        string? schedulePattern,
        ISchedulePatternParser schedulePatternParser)
    {
        if (string.IsNullOrWhiteSpace(preferredSchedule) || string.IsNullOrWhiteSpace(schedulePattern))
        {
            return true;
        }

        var criteria = ParsePreferredSchedule(preferredSchedule);
        if (!criteria.HasConstraints)
        {
            return true;
        }

        var slotParseResult = schedulePatternParser.ParseScheduleSlots(schedulePattern);
        if (slotParseResult.IsFailure || slotParseResult.Value.Count == 0)
        {
            return true;
        }

        var classSchedule = ParseSchedulePattern(slotParseResult.Value);

        if (criteria.IsWeekendPreference.HasValue)
        {
            var isWeekendClass = classSchedule.Days.Contains("SA") || classSchedule.Days.Contains("SU");
            if (isWeekendClass != criteria.IsWeekendPreference.Value)
            {
                return false;
            }
        }

        if (criteria.RequiredDays.Count > 0 &&
            !criteria.RequiredDays.All(requiredDay => classSchedule.Days.Contains(requiredDay)))
        {
            return false;
        }

        if (criteria.TimeBucket.HasValue &&
            !classSchedule.Slots.Any(slot => IsHourInBucket(slot.Hour, criteria.TimeBucket.Value)))
        {
            return false;
        }

        if (criteria.ExactHour.HasValue &&
            !classSchedule.Slots.Any(slot => slot.Hour == criteria.ExactHour.Value))
        {
            return false;
        }

        if (criteria.ExactMinute.HasValue &&
            !classSchedule.Slots.Any(slot => slot.Minute == criteria.ExactMinute.Value))
        {
            return false;
        }

        return true;
    }

    private static string? ResolveEffectiveWeeklyScheduleJson(Kidzgo.Domain.Classes.Class classEntity, DateOnly referenceDate)
    {
        return SchedulePatternSupport.ResolveEffectiveWeeklyScheduleJson(
            classEntity.WeeklyScheduleJson,
            classEntity.ScheduleSegments.Select(segment => new WeeklyScheduleSegmentWindow(
                segment.EffectiveFrom,
                segment.EffectiveTo,
                segment.WeeklyScheduleJson)),
            referenceDate);
    }

    private static PreferredScheduleCriteria ParsePreferredSchedule(string preferredSchedule)
    {
        var normalized = NormalizePreferredSchedule(preferredSchedule);
        var requiredDays = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddDayIfMatched(requiredDays, normalized, @"(?:\bthu\s*2\b|\bt\s*2\b|\bmon(?:day)?\b)", "MO");
        AddDayIfMatched(requiredDays, normalized, @"(?:\bthu\s*3\b|\bt\s*3\b|\btue(?:sday)?\b)", "TU");
        AddDayIfMatched(requiredDays, normalized, @"(?:\bthu\s*4\b|\bt\s*4\b|\bwed(?:nesday)?\b)", "WE");
        AddDayIfMatched(requiredDays, normalized, @"(?:\bthu\s*5\b|\bt\s*5\b|\bthursday\b)", "TH");
        AddDayIfMatched(requiredDays, normalized, @"(?:\bthu\s*6\b|\bt\s*6\b|\bfri(?:day)?\b)", "FR");
        AddDayIfMatched(requiredDays, normalized, @"(?:\bthu\s*7\b|\bt\s*7\b|\bsat(?:urday)?\b)", "SA");
        AddDayIfMatched(requiredDays, normalized, @"(?:\bchu\s*nhat\b|\bcn\b|\bsun(?:day)?\b)", "SU");

        bool? isWeekendPreference = null;
        if (Regex.IsMatch(normalized, @"(?:\bcuoi\s*tuan\b|\bweekend\b)", RegexOptions.IgnoreCase))
        {
            isWeekendPreference = true;
        }
        else if (Regex.IsMatch(normalized, @"(?:\btrong\s*tuan\b|\bweekdays?\b)", RegexOptions.IgnoreCase))
        {
            isWeekendPreference = false;
        }

        TimeBucket? timeBucket = null;
        if (Regex.IsMatch(normalized, @"(?:\bsang\b|\bmorning\b)", RegexOptions.IgnoreCase))
        {
            timeBucket = TimeBucket.Morning;
        }
        else if (Regex.IsMatch(normalized, @"(?:\bchieu\b|\bafternoon\b)", RegexOptions.IgnoreCase))
        {
            timeBucket = TimeBucket.Afternoon;
        }
        else if (Regex.IsMatch(normalized, @"(?:\btoi\b|\btui\b|\bevening\b|\bnight\b)", RegexOptions.IgnoreCase))
        {
            timeBucket = TimeBucket.Evening;
        }

        var explicitTime = ParseExplicitTime(normalized);
        if (explicitTime.Hour.HasValue && !timeBucket.HasValue)
        {
            timeBucket = MapHourToBucket(explicitTime.Hour.Value);
        }

        return new PreferredScheduleCriteria(
            requiredDays,
            isWeekendPreference,
            timeBucket,
            explicitTime.Hour,
            explicitTime.Minute);
    }

    private static SchedulePatternInfo ParseSchedulePattern(IReadOnlyCollection<ScheduleSlot> slots)
    {
        var normalizedSlots = slots
            .Select(slot =>
            {
                var parts = slot.StartTime.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var hour = parts.Length > 0 && int.TryParse(parts[0], out var parsedHour) ? parsedHour : -1;
                var minute = parts.Length > 1 && int.TryParse(parts[1], out var parsedMinute) ? parsedMinute : 0;

                return new ScheduleSlotInfo(slot.DayOfWeek, hour, minute);
            })
            .ToList();

        return new SchedulePatternInfo(
            normalizedSlots.Select(slot => slot.Day).ToHashSet(StringComparer.OrdinalIgnoreCase),
            normalizedSlots);
    }

    private static (int? Hour, int? Minute) ParseExplicitTime(string normalizedPreferredSchedule)
    {
        var hourMinuteMatch = Regex.Match(
            normalizedPreferredSchedule,
            @"(?<!\d)(?<hour>[01]?\d|2[0-3])\s*(?:[:h])\s*(?<minute>[0-5]\d)?\s*(?<ampm>am|pm)?\b",
            RegexOptions.IgnoreCase);

        if (hourMinuteMatch.Success)
        {
            return ParseMatchedTime(hourMinuteMatch);
        }

        var hourWordMatch = Regex.Match(
            normalizedPreferredSchedule,
            @"(?<!\d)(?<hour>[01]?\d|2[0-3])\s*(?:gio|g)\b(?:\s*(?<minute>[0-5]\d)\s*phut?)?\s*(?<ampm>am|pm)?",
            RegexOptions.IgnoreCase);

        if (hourWordMatch.Success)
        {
            return ParseMatchedTime(hourWordMatch);
        }

        var amPmMatch = Regex.Match(
            normalizedPreferredSchedule,
            @"(?<!\d)(?<hour>1[0-2]|0?\d)\s*(?<ampm>am|pm)\b",
            RegexOptions.IgnoreCase);

        return amPmMatch.Success
            ? ParseMatchedTime(amPmMatch)
            : (null, null);
    }

    private static (int? Hour, int? Minute) ParseMatchedTime(Match match)
    {
        if (!int.TryParse(match.Groups["hour"].Value, out var hour))
        {
            return (null, null);
        }

        var minute = 0;
        if (match.Groups["minute"].Success && !int.TryParse(match.Groups["minute"].Value, out minute))
        {
            minute = 0;
        }

        if (match.Groups["ampm"].Success)
        {
            var ampm = match.Groups["ampm"].Value.ToLowerInvariant();
            if (ampm == "pm" && hour < 12)
            {
                hour += 12;
            }
            else if (ampm == "am" && hour == 12)
            {
                hour = 0;
            }
        }

        return (hour, minute);
    }

    private static string NormalizePreferredSchedule(string preferredSchedule)
    {
        var normalized = preferredSchedule
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
    }

    private static void AddDayIfMatched(HashSet<string> requiredDays, string normalized, string pattern, string rruleDay)
    {
        if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
        {
            requiredDays.Add(rruleDay);
        }
    }

    private static bool IsHourInBucket(int hour, TimeBucket timeBucket)
    {
        return timeBucket switch
        {
            TimeBucket.Morning => hour >= 6 && hour < 12,
            TimeBucket.Afternoon => hour >= 12 && hour < 18,
            TimeBucket.Evening => hour >= 18 && hour < 22,
            _ => false
        };
    }

    private static TimeBucket? MapHourToBucket(int hour)
    {
        if (hour >= 6 && hour < 12)
        {
            return TimeBucket.Morning;
        }

        if (hour >= 12 && hour < 18)
        {
            return TimeBucket.Afternoon;
        }

        if (hour >= 18 && hour < 22)
        {
            return TimeBucket.Evening;
        }

        return null;
    }

    private sealed record PreferredScheduleCriteria(
        HashSet<string> RequiredDays,
        bool? IsWeekendPreference,
        TimeBucket? TimeBucket,
        int? ExactHour,
        int? ExactMinute)
    {
        public bool HasConstraints =>
            RequiredDays.Count > 0 ||
            IsWeekendPreference.HasValue ||
            TimeBucket.HasValue ||
            ExactHour.HasValue;
    }

    private sealed record SchedulePatternInfo(
        HashSet<string> Days,
        List<ScheduleSlotInfo> Slots);

    private sealed record ScheduleSlotInfo(
        string Day,
        int Hour,
        int Minute);

    private enum TimeBucket
    {
        Morning,
        Afternoon,
        Evening
    }
}
