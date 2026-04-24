using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.AddEnrollmentScheduleSegment;

public sealed class AddEnrollmentScheduleSegmentCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService)
    : ICommandHandler<AddEnrollmentScheduleSegmentCommand, AddEnrollmentScheduleSegmentResponse>
{
    public async Task<Result<AddEnrollmentScheduleSegmentResponse>> Handle(
        AddEnrollmentScheduleSegmentCommand command,
        CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.ScheduleSegments)
            .FirstOrDefaultAsync(e => e.Id == command.EnrollmentId, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.NotFound(command.EnrollmentId));
        }

        if (!enrollment.Class.Program.IsSupplementary)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.SupplementaryProgramRequired);
        }

        if (enrollment.Status == EnrollmentStatus.Dropped)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.AlreadyDropped);
        }

        if (command.EffectiveFrom < enrollment.EnrollDate)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveFrom cannot be earlier than the enrollment date."));
        }

        if (enrollment.Class.EndDate.HasValue && command.EffectiveFrom > enrollment.Class.EndDate.Value)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveFrom cannot be later than the class end date."));
        }

        if (command.EffectiveTo.HasValue && command.EffectiveTo.Value < command.EffectiveFrom)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveTo cannot be earlier than EffectiveFrom."));
        }

        if (command.EffectiveTo.HasValue &&
            enrollment.Class.EndDate.HasValue &&
            command.EffectiveTo.Value > enrollment.Class.EndDate.Value)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.InvalidScheduleSegmentEffectiveDate(
                    "EffectiveTo cannot be later than the class end date."));
        }

        string? sessionSelectionPattern;
        if (command.ClearWeeklyPattern)
        {
            sessionSelectionPattern = null;
        }
        else
        {
            var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
                command.WeeklyPattern,
                requireValue: false);
            if (weeklyPatternResult.IsFailure)
            {
                return Result.Failure<AddEnrollmentScheduleSegmentResponse>(weeklyPatternResult.Error);
            }

            sessionSelectionPattern = weeklyPatternResult.Value;
        }

        var validationResult = await studentSessionAssignmentService.ValidateSelectionPatternForPeriodAsync(
            enrollment.Class,
            sessionSelectionPattern,
            command.EffectiveFrom,
            command.EffectiveTo,
            cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(validationResult.Error);
        }

        var existingSegments = enrollment.ScheduleSegments
            .OrderBy(segment => segment.EffectiveFrom)
            .ToList();

        if (existingSegments.Any(segment => segment.EffectiveFrom == command.EffectiveFrom))
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.ScheduleSegmentAlreadyExists(command.EffectiveFrom));
        }

        if (existingSegments.Any(segment => segment.EffectiveFrom > command.EffectiveFrom))
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(
                EnrollmentErrors.FutureScheduleSegmentExists(command.EffectiveFrom));
        }

        var effectiveFromUtc = VietnamTime.TreatAsVietnamLocal(
            command.EffectiveFrom.ToDateTime(TimeOnly.MinValue));
        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            enrollment.StudentProfileId,
            enrollment.ClassId,
            command.EffectiveFrom,
            sessionSelectionPattern,
            cancellationToken,
            excludeEnrollmentId: enrollment.Id,
            excludeLegacyClassId: enrollment.ClassId,
            excludeSlotsFromUtc: effectiveFromUtc);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<AddEnrollmentScheduleSegmentResponse>(conflictResult.Error);
        }

        var now = VietnamTime.UtcNow();
        var today = VietnamTime.TodayDateOnly();
        if (existingSegments.Count == 0 && command.EffectiveFrom > enrollment.EnrollDate)
        {
            context.ClassEnrollmentScheduleSegments.Add(new ClassEnrollmentScheduleSegment
            {
                Id = Guid.NewGuid(),
                ClassEnrollmentId = enrollment.Id,
                EffectiveFrom = enrollment.EnrollDate,
                EffectiveTo = command.EffectiveFrom.AddDays(-1),
                SessionSelectionPattern = enrollment.SessionSelectionPattern,
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

        var newSegment = new ClassEnrollmentScheduleSegment
        {
            Id = Guid.NewGuid(),
            ClassEnrollmentId = enrollment.Id,
            EffectiveFrom = command.EffectiveFrom,
            EffectiveTo = command.EffectiveTo,
            SessionSelectionPattern = sessionSelectionPattern,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollmentScheduleSegments.Add(newSegment);
        if (command.EffectiveFrom <= today || command.EffectiveFrom == enrollment.EnrollDate)
        {
            enrollment.SessionSelectionPattern = sessionSelectionPattern;
        }

        enrollment.UpdatedAt = now;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AddEnrollmentScheduleSegmentResponse
        {
            Id = newSegment.Id,
            EnrollmentId = enrollment.Id,
            ClassId = enrollment.ClassId,
            ProgramId = enrollment.Class.ProgramId,
            EffectiveFrom = newSegment.EffectiveFrom,
            EffectiveTo = newSegment.EffectiveTo,
            WeeklyPattern = ParseWeeklyPatternOrNull(newSegment.SessionSelectionPattern),
            ActiveWeeklyPattern = ParseWeeklyPatternOrNull(enrollment.SessionSelectionPattern)
        };
    }

    private static List<Kidzgo.Application.Abstraction.Services.WeeklyPatternEntry>? ParseWeeklyPatternOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var result = SchedulePatternSupport.ParseWeeklyPattern(value);
        return result.IsSuccess ? result.Value : null;
    }
}
