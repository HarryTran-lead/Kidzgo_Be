using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Abstraction.Services;

public interface ISchedulePatternParser
{
    Result<List<DateTime>> ParseAndGenerateOccurrences(string schedulePattern, DateOnly startDate, DateOnly? endDate);

    Result<List<ScheduleOccurrence>> ParseAndGenerateOccurrenceDetails(string schedulePattern, DateOnly startDate, DateOnly? endDate);

    int? ParseDuration(string schedulePattern);

    Result<List<ScheduleSlot>> ParseScheduleSlots(string schedulePattern);
}
