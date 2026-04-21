using Kidzgo.Application.Enrollments.BackfillStudentSessionAssignments;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class BackfillStudentSessionAssignmentsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<BackfillStudentSessionAssignmentsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new BackfillStudentSessionAssignmentsCommand(),
            context.CancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "BackfillStudentSessionAssignmentsJob failed: {ErrorCode} - {ErrorDescription}",
                result.Error.Code,
                result.Error.Description);
            return;
        }

        logger.LogInformation(
            "BackfillStudentSessionAssignmentsJob processed {ProcessedEnrollments}/{MatchedEnrollments} enrollments across {AffectedClasses} classes. Created {CreatedAssignments}, reactivated {ReactivatedAssignments}, cancelled {CancelledAssignments} assignments.",
            result.Value.ProcessedEnrollments,
            result.Value.MatchedEnrollments,
            result.Value.AffectedClasses,
            result.Value.CreatedAssignments,
            result.Value.ReactivatedAssignments,
            result.Value.CancelledAssignments);
    }
}
