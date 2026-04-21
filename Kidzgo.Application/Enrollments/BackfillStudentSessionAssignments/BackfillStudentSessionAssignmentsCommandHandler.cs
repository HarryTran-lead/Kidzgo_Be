using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.BackfillStudentSessionAssignments;

public sealed class BackfillStudentSessionAssignmentsCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService)
    : ICommandHandler<BackfillStudentSessionAssignmentsCommand, BackfillStudentSessionAssignmentsResponse>
{
    private const int DefaultBatchSize = 100;
    private const int MaxBatchSize = 500;

    public async Task<Result<BackfillStudentSessionAssignmentsResponse>> Handle(
        BackfillStudentSessionAssignmentsCommand command,
        CancellationToken cancellationToken)
    {
        var batchSize = NormalizeBatchSize(command.BatchSize);

        var enrollmentQuery = context.ClassEnrollments
            .AsNoTracking()
            .Where(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused);

        if (command.EnrollmentId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.Id == command.EnrollmentId.Value);
        }

        if (command.ClassId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.ClassId == command.ClassId.Value);
        }

        if (command.StudentProfileId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.StudentProfileId == command.StudentProfileId.Value);
        }

        var matchedEnrollments = await enrollmentQuery
            .OrderBy(e => e.ClassId)
            .ThenBy(e => e.EnrollDate)
            .Select(e => new EnrollmentSnapshot(e.Id, e.ClassId))
            .ToListAsync(cancellationToken);

        if (matchedEnrollments.Count == 0)
        {
            return Result.Success(new BackfillStudentSessionAssignmentsResponse
            {
                MatchedEnrollments = 0,
                ProcessedEnrollments = 0,
                AffectedClasses = 0,
                BatchSize = batchSize
            });
        }

        var affectedClassIds = new HashSet<Guid>();
        var processedEnrollments = 0;
        var createdAssignments = 0;
        var reactivatedAssignments = 0;
        var cancelledAssignments = 0;

        foreach (var batch in Batch(matchedEnrollments, batchSize))
        {
            var enrollmentIds = batch
                .Select(item => item.Id)
                .ToList();

            var enrollments = await context.ClassEnrollments
                .Where(e => enrollmentIds.Contains(e.Id))
                .ToListAsync(cancellationToken);

            var enrollmentLookup = enrollments.ToDictionary(e => e.Id);

            foreach (var snapshot in batch)
            {
                if (!enrollmentLookup.TryGetValue(snapshot.Id, out var enrollment))
                {
                    continue;
                }

                var syncSummary = await studentSessionAssignmentService
                    .SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);

                affectedClassIds.Add(snapshot.ClassId);
                processedEnrollments++;
                createdAssignments += syncSummary.CreatedAssignments;
                reactivatedAssignments += syncSummary.ReactivatedAssignments;
                cancelledAssignments += syncSummary.CancelledAssignments;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new BackfillStudentSessionAssignmentsResponse
        {
            MatchedEnrollments = matchedEnrollments.Count,
            ProcessedEnrollments = processedEnrollments,
            AffectedClasses = affectedClassIds.Count,
            BatchSize = batchSize,
            CreatedAssignments = createdAssignments,
            ReactivatedAssignments = reactivatedAssignments,
            CancelledAssignments = cancelledAssignments
        });
    }

    private static int NormalizeBatchSize(int? batchSize)
    {
        if (!batchSize.HasValue || batchSize.Value <= 0)
        {
            return DefaultBatchSize;
        }

        return batchSize.Value > MaxBatchSize
            ? MaxBatchSize
            : batchSize.Value;
    }

    private static IEnumerable<List<EnrollmentSnapshot>> Batch(
        IReadOnlyList<EnrollmentSnapshot> items,
        int batchSize)
    {
        for (var index = 0; index < items.Count; index += batchSize)
        {
            yield return items
                .Skip(index)
                .Take(batchSize)
                .ToList();
        }
    }

    private sealed record EnrollmentSnapshot(Guid Id, Guid ClassId);
}
