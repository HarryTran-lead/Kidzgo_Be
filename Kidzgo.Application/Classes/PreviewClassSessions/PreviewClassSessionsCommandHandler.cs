using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.PreviewClassSessions;

public sealed class PreviewClassSessionsCommandHandler(
    IDbContext context,
    ISchedulePatternParser patternParser,
    ClassSessionPlanningService classSessionPlanningService
) : ICommandHandler<PreviewClassSessionsCommand, PreviewClassSessionsResponse>
{
    private const int OccurrenceBatchDays = 56;
    private const int MaxOccurrenceSearchDays = 730;

    public async Task<Result<PreviewClassSessionsResponse>> Handle(
        PreviewClassSessionsCommand command,
        CancellationToken cancellationToken)
    {
        var level = await context.Levels
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.LevelId, cancellationToken);
        if (level is null)
        {
            return Result.Failure<PreviewClassSessionsResponse>(ClassErrors.LevelNotFound);
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<PreviewClassSessionsResponse>(ClassErrors.LevelProgramMismatch);
        }

        var normalizedPatternResult = SchedulePatternSupport.NormalizeWeeklyScheduleJson(
            command.WeeklyScheduleSlots,
            requireValue: true);
        if (normalizedPatternResult.IsFailure)
        {
            return Result.Failure<PreviewClassSessionsResponse>(normalizedPatternResult.Error);
        }

        var occurrencesResult = await BuildOccurrencesAsync(
            normalizedPatternResult.Value!,
            command.StartDate,
            command.EndDate,
            command.SessionsToGenerate,
            command.SkipHolidays,
            cancellationToken);
        if (occurrencesResult.IsFailure)
        {
            return Result.Failure<PreviewClassSessionsResponse>(occurrencesResult.Error);
        }

        var plannedMetadataResult = await classSessionPlanningService.PlanAsync(
            command.LevelId,
            command.StartModuleId,
            command.StartSessionIndex,
            existingSessionCount: 0,
            newSessionCount: command.SessionsToGenerate,
            strictCurriculumCoverage: true,
            cancellationToken);
        if (plannedMetadataResult.IsFailure)
        {
            return Result.Failure<PreviewClassSessionsResponse>(plannedMetadataResult.Error);
        }

        var occurrences = occurrencesResult.Value;
        var plannedMetadata = plannedMetadataResult.Value;

        return new PreviewClassSessionsResponse
        {
            ExpectedEndDate = occurrences.Count > 0
                ? VietnamTime.ToVietnamDateOnly(occurrences[^1].PlannedDatetime)
                : null,
            Sessions = occurrences
                .Select((occurrence, index) => new PreviewClassSessionItem
                {
                    ClassSessionNo = plannedMetadata[index].ClassSessionNo,
                    Date = VietnamTime.ToVietnamDateOnly(occurrence.PlannedDatetime),
                    ModuleName = plannedMetadata[index].ModuleName ?? string.Empty,
                    UnitName = plannedMetadata[index].UnitName,
                    LessonTitle = plannedMetadata[index].LessonTitle,
                    CurriculumSessionIndex = plannedMetadata[index].SessionIndexInModule ?? 0
                })
                .ToList()
        };
    }

    private async Task<Result<List<ScheduleOccurrence>>> BuildOccurrencesAsync(
        string weeklyScheduleJson,
        DateOnly startDate,
        DateOnly? endDate,
        int sessionsToGenerate,
        bool skipHolidays,
        CancellationToken cancellationToken)
    {
        var occurrences = new List<ScheduleOccurrence>(sessionsToGenerate);
        var searchStart = startDate;
        var absoluteSearchEnd = endDate ?? startDate.AddDays(MaxOccurrenceSearchDays);

        while (occurrences.Count < sessionsToGenerate && searchStart <= absoluteSearchEnd)
        {
            var batchEnd = searchStart.AddDays(OccurrenceBatchDays - 1);
            if (batchEnd > absoluteSearchEnd)
            {
                batchEnd = absoluteSearchEnd;
            }

            var parseResult = patternParser.ParseAndGenerateOccurrenceDetails(
                weeklyScheduleJson,
                searchStart,
                batchEnd);

            if (parseResult.IsFailure)
            {
                return Result.Failure<List<ScheduleOccurrence>>(parseResult.Error);
            }

            HashSet<DateOnly> holidayDates = [];
            if (skipHolidays)
            {
                var holidays = await context.Holidays
                    .AsNoTracking()
                    .Where(h => h.IsActive && h.StartDate <= batchEnd && h.EndDate >= searchStart)
                    .ToListAsync(cancellationToken);

                foreach (var holiday in holidays)
                {
                    for (var date = holiday.StartDate; date <= holiday.EndDate; date = date.AddDays(1))
                    {
                        holidayDates.Add(date);
                    }
                }
            }

            foreach (var occurrence in parseResult.Value.OrderBy(x => x.PlannedDatetime))
            {
                var occurrenceDate = VietnamTime.ToVietnamDateOnly(occurrence.PlannedDatetime);
                if (skipHolidays && holidayDates.Contains(occurrenceDate))
                {
                    continue;
                }

                if (occurrences.Any(x => x.PlannedDatetime == occurrence.PlannedDatetime))
                {
                    continue;
                }

                occurrences.Add(occurrence);
                if (occurrences.Count == sessionsToGenerate)
                {
                    break;
                }
            }

            searchStart = batchEnd.AddDays(1);
        }

        if (occurrences.Count < sessionsToGenerate)
        {
            return Result.Failure<List<ScheduleOccurrence>>(Error.Validation(
                "Class.NotEnoughScheduleOccurrences",
                $"Could only generate {occurrences.Count} scheduled occurrence(s) from {startDate:yyyy-MM-dd} but {sessionsToGenerate} were requested."));
        }

        return Result.Success(occurrences);
    }
}
