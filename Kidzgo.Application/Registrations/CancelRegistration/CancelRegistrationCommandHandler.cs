using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.CancelRegistration.Handler;

public sealed class CancelRegistrationCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    TicketGrantService ticketGrantService
) : ICommandHandler<CancelRegistrationCommand, CancelRegistrationResponse>
{
    public async Task<Result<CancelRegistrationResponse>> Handle(
        CancelRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var normalizedReason = string.IsNullOrWhiteSpace(command.Reason)
            ? null
            : command.Reason.Trim();

        var registration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<CancelRegistrationResponse>(RegistrationErrors.NotFound(command.Id));
        }

        // Validate status - cannot cancel if already completed or cancelled
        if (registration.Status == RegistrationStatus.Completed || 
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<CancelRegistrationResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "cancel"));
        }

        var enrollments = await context.ClassEnrollments
            .Where(ce => ce.StudentProfileId == registration.StudentProfileId
                && ce.Status == Domain.Classes.EnrollmentStatus.Active
                && (ce.RegistrationId == registration.Id ||
                    (!ce.RegistrationId.HasValue &&
                     (ce.ClassId == registration.ClassId || ce.ClassId == registration.SecondaryClassId))))
            .ToListAsync(cancellationToken);
        var impactedClassIds = enrollments
            .Select(e => e.ClassId)
            .Distinct()
            .ToList();

        foreach (var enrollment in enrollments)
        {
            enrollment.Status = Domain.Classes.EnrollmentStatus.Dropped;
            enrollment.UpdatedAt = now;
            await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                enrollment.Id,
                now,
                cancellationToken);
        }

        var voidReason = normalizedReason is null
            ? "Void remaining tickets because registration was cancelled"
            : $"Void remaining tickets because registration was cancelled: {normalizedReason}";

        await ticketGrantService.VoidAvailableTicketsAsync(
            registration.StudentProfileId,
            registration.Id,
            voidReason,
            createdByUserId: null,
            cancellationToken);

        // Update registration status
        registration.Status = RegistrationStatus.Cancelled;
        if (normalizedReason is not null)
        {
            registration.Note = string.IsNullOrEmpty(registration.Note)
                ? normalizedReason
                : $"{registration.Note} | Cancel reason: {normalizedReason}";
        }

        registration.RemainingSessions = 0;
        registration.TotalSessions = registration.UsedSessions;
        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        foreach (var classId in impactedClassIds)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(classId, cancellationToken);
        }

        if (impactedClassIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return new CancelRegistrationResponse
        {
            Id = registration.Id,
            Status = registration.Status.ToString(),
            Reason = normalizedReason,
            CancelledAt = now
        };
    }
}
