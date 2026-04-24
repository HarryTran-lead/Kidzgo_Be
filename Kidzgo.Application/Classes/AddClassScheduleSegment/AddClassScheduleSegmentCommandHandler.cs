using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.AddClassScheduleSegment;

public sealed class AddClassScheduleSegmentCommandHandler(
    IDbContext context,
    ISchedulePatternParser patternParser,
    SessionGenerationService sessionGenerationService)
    : ICommandHandler<AddClassScheduleSegmentCommand, AddClassScheduleSegmentResponse>
{
    public async Task<Result<AddClassScheduleSegmentResponse>> Handle(
        AddClassScheduleSegmentCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedPatternResult = SchedulePatternSupport.NormalizeWeeklyScheduleJson(
            command.WeeklyScheduleSlots,
            requireValue: true);
        if (normalizedPatternResult.IsFailure)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(normalizedPatternResult.Error);
        }

        var normalizedWeeklyScheduleJson = normalizedPatternResult.Value!;

        var classEntity = await context.Classes
            .Include(c => c.Program)
            .Include(c => c.ScheduleSegments)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.NotFound(command.ClassId));
        }

        if (!classEntity.Program.IsSupplementary)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.SupplementaryProgramRequired);
        }

        if (command.EffectiveFrom < classEntity.StartDate)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveFrom cannot be earlier than the class start date."));
        }

        if (classEntity.EndDate.HasValue && command.EffectiveFrom > classEntity.EndDate.Value)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveFrom cannot be later than the class end date."));
        }

        if (command.EffectiveTo.HasValue && command.EffectiveTo.Value < command.EffectiveFrom)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveTo cannot be earlier than EffectiveFrom."));
        }

        if (command.EffectiveTo.HasValue &&
            classEntity.EndDate.HasValue &&
            command.EffectiveTo.Value > classEntity.EndDate.Value)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveTo cannot be later than the class end date."));
        }

        var validationEndDate = command.EffectiveTo
            ?? classEntity.EndDate
            ?? command.EffectiveFrom.AddMonths(3);
        var parseResult = patternParser.ParseAndGenerateOccurrenceDetails(
            normalizedWeeklyScheduleJson,
            command.EffectiveFrom,
            validationEndDate);
        if (parseResult.IsFailure)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(parseResult.Error);
        }

        if (parseResult.Value.Count == 0)
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.InvalidScheduleSegmentEffectiveDate(
                    "Weekly schedule does not generate any sessions in the effective range."));
        }

        var existingSegments = classEntity.ScheduleSegments
            .OrderBy(segment => segment.EffectiveFrom)
            .ToList();

        if (existingSegments.Any(segment => segment.EffectiveFrom == command.EffectiveFrom))
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.ScheduleSegmentAlreadyExists(command.EffectiveFrom));
        }

        if (existingSegments.Any(segment => segment.EffectiveFrom > command.EffectiveFrom))
        {
            return Result.Failure<AddClassScheduleSegmentResponse>(
                ClassErrors.FutureScheduleSegmentExists(command.EffectiveFrom));
        }

        var now = VietnamTime.UtcNow();
        var today = VietnamTime.TodayDateOnly();
        if (existingSegments.Count == 0 && command.EffectiveFrom > classEntity.StartDate)
        {
            if (string.IsNullOrWhiteSpace(classEntity.WeeklyScheduleJson))
            {
                return Result.Failure<AddClassScheduleSegmentResponse>(
                    SessionErrors.MissingSchedulePattern(classEntity.Id));
            }

            context.ClassScheduleSegments.Add(new ClassScheduleSegment
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                EffectiveFrom = classEntity.StartDate,
                EffectiveTo = command.EffectiveFrom.AddDays(-1),
                WeeklyScheduleJson = classEntity.WeeklyScheduleJson,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            var currentSegment = existingSegments
                .LastOrDefault(segment => segment.EffectiveFrom < command.EffectiveFrom &&
                    (!segment.EffectiveTo.HasValue || command.EffectiveFrom <= segment.EffectiveTo.Value));

            if (currentSegment is not null)
            {
                currentSegment.EffectiveTo = command.EffectiveFrom.AddDays(-1);
                currentSegment.UpdatedAt = now;
            }
        }

        var newSegment = new ClassScheduleSegment
        {
            Id = Guid.NewGuid(),
            ClassId = classEntity.Id,
            EffectiveFrom = command.EffectiveFrom,
            EffectiveTo = command.EffectiveTo,
            WeeklyScheduleJson = normalizedWeeklyScheduleJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        var shouldUpdateCurrentWeeklySchedule =
            command.EffectiveFrom <= today ||
            command.EffectiveFrom == classEntity.StartDate;

        context.ClassScheduleSegments.Add(newSegment);
        if (shouldUpdateCurrentWeeklySchedule)
        {
            classEntity.WeeklyScheduleJson = normalizedWeeklyScheduleJson;
        }

        classEntity.UpdatedAt = now;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var generatedSessionsCount = 0;
        if (command.GenerateSessions)
        {
            var generateResult = await sessionGenerationService.GenerateSessionsFromPatternAsync(
                classEntity,
                command.OnlyFutureSessions,
                cancellationToken);
            if (generateResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<AddClassScheduleSegmentResponse>(generateResult.Error);
            }

            generatedSessionsCount = generateResult.Value;
        }

        await transaction.CommitAsync(cancellationToken);

        return new AddClassScheduleSegmentResponse
        {
            Id = newSegment.Id,
            ClassId = classEntity.Id,
            ProgramId = classEntity.ProgramId,
            EffectiveFrom = newSegment.EffectiveFrom,
            EffectiveTo = newSegment.EffectiveTo,
            WeeklyScheduleSlots = patternParser.ParseScheduleSlots(newSegment.WeeklyScheduleJson).Value,
            GeneratedSessionsCount = generatedSessionsCount
        };
    }
}
