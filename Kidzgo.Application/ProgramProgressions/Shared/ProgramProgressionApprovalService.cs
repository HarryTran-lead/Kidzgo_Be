using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.Shared;

public sealed record ProgramProgressionApprovalResult(
    Guid AssessmentId,
    Guid? GeneratedRegistrationId);

public sealed class ProgramProgressionApprovalService(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService)
{
    public async Task<Result<ProgramProgressionApprovalResult>> ApproveAsync(
        Guid assessmentId,
        Guid? tuitionPlanId,
        string? approvalNote,
        Guid? approvedByUserId,
        CancellationToken cancellationToken)
    {
        var assessment = await context.ProgramProgressionAssessments
            .Include(x => x.Rule)
            .FirstOrDefaultAsync(x => x.Id == assessmentId, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(
                ProgramProgressionErrors.AssessmentNotFound(assessmentId));
        }

        if (assessment.Status == ProgramProgressionAssessmentStatus.Approved)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(
                ProgramProgressionErrors.AssessmentAlreadyApproved(assessment.Id));
        }

        if (!assessment.IsEligible)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(
                ProgramProgressionErrors.AssessmentNotEligible(assessment.Id));
        }

        var sourceRegistration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r => r.Id == assessment.SourceRegistrationId, cancellationToken);

        if (sourceRegistration is null)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(
                ProgramProgressionErrors.RegistrationNotFound(assessment.SourceRegistrationId));
        }

        if (sourceRegistration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(
                ProgramProgressionErrors.InvalidRegistrationStatus(sourceRegistration.Status.ToString()));
        }

        if (sourceRegistration.ProgramId != assessment.SourceProgramId)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(
                ProgramProgressionErrors.SourceProgramMismatch(sourceRegistration.Id, assessment.SourceProgramId));
        }

        if (sourceRegistration.LevelId != assessment.SourceLevelId)
        {
            return Result.Failure<ProgramProgressionApprovalResult>(Error.Validation(
                "ProgramProgression.SourceLevelMismatch",
                $"Registration '{sourceRegistration.Id}' does not match source level '{assessment.SourceLevelId}'."));
        }

        var now = VietnamTime.UtcNow();
        Guid? generatedRegistrationId = null;
        Guid? resolvedTargetLevelId = assessment.TargetLevelId;

        if (assessment.TargetProgramId.HasValue)
        {
            if (!tuitionPlanId.HasValue)
            {
                return Result.Failure<ProgramProgressionApprovalResult>(Error.Validation(
                    "ProgramProgression.TuitionPlanRequired",
                    "A tuition plan must be selected when approving progression to the next program."));
            }

            var targetProgramId = assessment.TargetProgramId.Value;
            var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
                context,
                sourceRegistration.BranchId,
                targetProgramId,
                cancellationToken);

            if (!programAssignedToBranch)
            {
                return Result.Failure<ProgramProgressionApprovalResult>(
                    RegistrationErrors.ProgramNotAvailableInBranch(targetProgramId, sourceRegistration.BranchId));
            }

            var selectedTuitionPlan = await context.TuitionPlans
                .Include(tp => tp.Program)
                .FirstOrDefaultAsync(tp =>
                    tp.Id == tuitionPlanId.Value &&
                    tp.ProgramId == targetProgramId &&
                    tp.IsActive &&
                    !tp.IsDeleted,
                    cancellationToken);

            if (selectedTuitionPlan is null)
            {
                return Result.Failure<ProgramProgressionApprovalResult>(
                    RegistrationErrors.TuitionPlanNotFound(tuitionPlanId.Value));
            }

            var targetLevelId = resolvedTargetLevelId;

            if (targetLevelId.HasValue &&
                selectedTuitionPlan.LevelId.HasValue &&
                selectedTuitionPlan.LevelId.Value != targetLevelId.Value)
            {
                return Result.Failure<ProgramProgressionApprovalResult>(Error.Validation(
                    "ProgramProgression.TuitionPlanLevelMismatch",
                    "Selected tuition plan does not match the target level in progression rule."));
            }

            if (!targetLevelId.HasValue)
            {
                targetLevelId = selectedTuitionPlan.LevelId;
            }

            if (targetLevelId.HasValue)
            {
                var targetLevelExists = await context.Levels
                    .AnyAsync(
                        x => x.Id == targetLevelId.Value &&
                             x.ProgramId == targetProgramId &&
                             x.IsActive,
                        cancellationToken);

                if (!targetLevelExists)
                {
                    return Result.Failure<ProgramProgressionApprovalResult>(Error.Validation(
                        "ProgramProgression.TargetLevelNotFound",
                        "Target level from progression rule was not found, inactive, or not in target program."));
                }
            }

            if (!targetLevelId.HasValue)
            {
                targetLevelId = await context.Levels
                    .Where(x => x.ProgramId == targetProgramId && x.IsActive)
                    .OrderBy(x => x.Order)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (!targetLevelId.HasValue)
            {
                return Result.Failure<ProgramProgressionApprovalResult>(
                    Error.Validation(
                        "ProgramProgression.TargetLevelNotFound",
                        "Cannot create registration because the target program has no active level."));
            }

            var duplicateRegistrationExists = await context.Registrations
                .AnyAsync(r =>
                    r.StudentProfileId == sourceRegistration.StudentProfileId &&
                    r.Id != sourceRegistration.Id &&
                    r.Status != RegistrationStatus.Completed &&
                    r.Status != RegistrationStatus.Cancelled &&
                    (r.ProgramId == targetProgramId || r.SecondaryProgramId == targetProgramId),
                    cancellationToken);

            if (duplicateRegistrationExists)
            {
                return Result.Failure<ProgramProgressionApprovalResult>(
                    RegistrationErrors.AlreadyExists(sourceRegistration.StudentProfileId, targetProgramId));
            }

            var carriedForwardSessions = assessment.Rule.CarryOverRemainingSessions
                ? Math.Max(sourceRegistration.RemainingSessions, 0)
                : 0;
            var totalSessions = selectedTuitionPlan.TotalSessions + carriedForwardSessions;
            var pricing = await RegistrationDiscountPricingHelper.ResolveAsync(
                context,
                sourceRegistration.BranchId,
                targetProgramId,
                selectedTuitionPlan.Id,
                OperationType.Promotion,
                now,
                selectedTuitionPlan.TuitionAmount,
                0m,
                cancellationToken);

            var newRegistration = new Registration
            {
                Id = Guid.NewGuid(),
                StudentProfileId = sourceRegistration.StudentProfileId,
                BranchId = sourceRegistration.BranchId,
                ProgramId = targetProgramId,
                LevelId = targetLevelId.Value,
                TuitionPlanId = selectedTuitionPlan.Id,
                RegistrationDate = now,
                ExpectedStartDate = now,
                PreferredSchedule = sourceRegistration.PreferredSchedule,
                Note = BuildProgressionNote(
                    sourceRegistration.Program.Name,
                    selectedTuitionPlan.Program.Name,
                    assessment.Id,
                    approvalNote),
                Status = RegistrationStatus.WaitingForClass,
                OriginalRegistrationId = sourceRegistration.Id,
                OperationType = OperationType.Promotion,
                TotalSessions = totalSessions,
                UsedSessions = 0,
                RemainingSessions = totalSessions,
                CreatedAt = now,
                UpdatedAt = now
            };
            RegistrationDiscountPricingHelper.ApplyToRegistration(newRegistration, pricing);
            context.Registrations.Add(newRegistration);
            generatedRegistrationId = newRegistration.Id;
            resolvedTargetLevelId = targetLevelId;

            if (assessment.Rule.CarryOverRemainingSessions)
            {
                sourceRegistration.RemainingSessions = 0;
            }
        }

        if (assessment.Rule.StopCurrentEnrollmentOnApproval)
        {
            var activeEnrollments = await context.ClassEnrollments
                .Where(e =>
                    e.StudentProfileId == sourceRegistration.StudentProfileId &&
                    e.RegistrationId == sourceRegistration.Id &&
                    e.Status == Domain.Classes.EnrollmentStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var enrollment in activeEnrollments)
            {
                enrollment.Status = Domain.Classes.EnrollmentStatus.Completed;
                enrollment.UpdatedAt = now;
                await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                    enrollment.Id,
                    now,
                    cancellationToken);
            }
        }

        sourceRegistration.Status = RegistrationStatus.Completed;
        sourceRegistration.UpdatedAt = now;

        assessment.Status = ProgramProgressionAssessmentStatus.Approved;
        assessment.ApprovedBy = approvedByUserId;
        assessment.ApprovedAt = now;
        assessment.ApprovalNote = string.IsNullOrWhiteSpace(approvalNote)
            ? null
            : approvalNote.Trim();
        assessment.ApprovedTuitionPlanId = generatedRegistrationId.HasValue ? tuitionPlanId : null;
        assessment.TargetLevelId = resolvedTargetLevelId;
        assessment.GeneratedRegistrationId = generatedRegistrationId;
        assessment.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProgramProgressionApprovalResult(
            assessment.Id,
            generatedRegistrationId));
    }

    private static string BuildProgressionNote(
        string sourceProgramName,
        string? targetProgramText,
        Guid assessmentId,
        string? approvalNote)
    {
        var baseNote = targetProgramText is null
            ? $"Hoan thanh chuong trinh '{sourceProgramName}' qua danh gia progression. Assessment ID: {assessmentId}."
            : $"Len chuong trinh moi tu '{sourceProgramName}' sang '{targetProgramText}'. Assessment ID: {assessmentId}.";

        if (string.IsNullOrWhiteSpace(approvalNote))
        {
            return baseNote;
        }

        return $"{baseNote} Ghi chu phe duyet: {approvalNote.Trim()}";
    }
}
