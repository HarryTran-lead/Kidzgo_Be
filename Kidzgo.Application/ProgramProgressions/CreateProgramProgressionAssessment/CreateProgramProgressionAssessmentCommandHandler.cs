using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionAssessment;

public sealed class CreateProgramProgressionAssessmentCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ProgramProgressionEvaluationService evaluationService)
    : ICommandHandler<CreateProgramProgressionAssessmentCommand, ProgramProgressionAssessmentDto>
{
    public async Task<Result<ProgramProgressionAssessmentDto>> Handle(
        CreateProgramProgressionAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        var currentUserRole = await ProgramProgressionAccessHelper.GetCurrentUserRoleAsync(
            context,
            userContext.UserId,
            cancellationToken);

        Domain.Registrations.Registration? registration;
        Domain.Classes.ClassEnrollment? sourceEnrollment = null;
        ProgramProgressionScheduleParticipant? scheduleParticipant = null;

        if (command.ScheduleParticipantId.HasValue)
        {
            scheduleParticipant = await context.ProgramProgressionScheduleParticipants
                .Include(participant => participant.Schedule)
                    .ThenInclude(schedule => schedule.Participants)
                .Include(participant => participant.SourceRegistration)
                    .ThenInclude(registration => registration.Program)
                .Include(participant => participant.SourceEnrollment)
                    .ThenInclude(enrollment => enrollment!.Class)
                .Include(participant => participant.Assessment)
                .FirstOrDefaultAsync(
                    participant => participant.Id == command.ScheduleParticipantId.Value,
                    cancellationToken);

            if (scheduleParticipant is null)
            {
                return Result.Failure<ProgramProgressionAssessmentDto>(
                    ProgramProgressionErrors.ScheduleParticipantNotFound(command.ScheduleParticipantId.Value));
            }

            if (scheduleParticipant.Assessment is not null)
            {
                return Result.Failure<ProgramProgressionAssessmentDto>(
                    ProgramProgressionErrors.AssessmentAlreadyLinkedToScheduleParticipant(scheduleParticipant.Id));
            }

            if (scheduleParticipant.Status != ProgramProgressionScheduleParticipantStatus.Scheduled)
            {
                return Result.Failure<ProgramProgressionAssessmentDto>(
                    ProgramProgressionErrors.ScheduleParticipantInvalidStatus(
                        scheduleParticipant.Id,
                        scheduleParticipant.Status.ToString()));
            }

            if (currentUserRole == Domain.Users.UserRole.Teacher)
            {
                var teacherAccess = await ProgramProgressionAccessHelper.EnsureTeacherAssignedToScheduleAsync(
                    context,
                    userContext.UserId,
                    scheduleParticipant.ScheduleId,
                    cancellationToken);
                if (teacherAccess.IsFailure)
                {
                    return Result.Failure<ProgramProgressionAssessmentDto>(teacherAccess.Error);
                }
            }

            registration = scheduleParticipant.SourceRegistration;
            sourceEnrollment = scheduleParticipant.SourceEnrollment;
        }
        else if (command.SourceRegistrationId.HasValue)
        {
            registration = await context.Registrations
                .Include(r => r.Program)
                .FirstOrDefaultAsync(r => r.Id == command.SourceRegistrationId.Value, cancellationToken);

            if (registration is null)
            {
                return Result.Failure<ProgramProgressionAssessmentDto>(
                    ProgramProgressionErrors.RegistrationNotFound(command.SourceRegistrationId.Value));
            }

            sourceEnrollment = await context.ClassEnrollments
                .Include(enrollment => enrollment.Class)
                .Where(enrollment => enrollment.RegistrationId == registration.Id)
                .OrderByDescending(enrollment => enrollment.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUserRole == Domain.Users.UserRole.Teacher)
            {
                if (sourceEnrollment?.ClassId is not Guid sourceClassId)
                {
                    return Result.Failure<ProgramProgressionAssessmentDto>(
                        ProgramProgressionErrors.TeacherCannotManageAssessment(userContext.UserId, Guid.Empty));
                }

                var teacherAccess = await ProgramProgressionAccessHelper.EnsureTeacherCanManageClassAssessmentAsync(
                    context,
                    userContext.UserId,
                    sourceClassId,
                    cancellationToken);
                if (teacherAccess.IsFailure)
                {
                    return Result.Failure<ProgramProgressionAssessmentDto>(teacherAccess.Error);
                }
            }
        }
        else
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(ProgramProgressionErrors.SourceRegistrationRequired);
        }

        if (registration.Status == Domain.Registrations.RegistrationStatus.Cancelled)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(
                ProgramProgressionErrors.InvalidRegistrationStatus(registration.Status.ToString()));
        }

        var rule = await context.ProgramProgressionRules
            .Include(r => r.SourceProgram)
            .Include(r => r.TargetProgram)
            .FirstOrDefaultAsync(r => r.SourceProgramId == registration.ProgramId && r.IsActive, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(
                ProgramProgressionErrors.NoActiveRuleForProgram(registration.ProgramId));
        }

        var evaluationResult = evaluationService.Evaluate(
            rule,
            new ProgramProgressionAssessmentInput(
                command.PassedInClass,
                command.ListeningScore,
                command.SpeakingScore,
                command.ReadingWritingScore,
                command.ReadingScore,
                command.WritingScore));
        if (evaluationResult.IsFailure)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(evaluationResult.Error);
        }

        var now = VietnamTime.UtcNow();
        var computed = evaluationResult.Value;
        var assessment = new ProgramProgressionAssessment
        {
            Id = Guid.NewGuid(),
            RuleId = rule.Id,
            ScheduleParticipantId = scheduleParticipant?.Id,
            StudentProfileId = registration.StudentProfileId,
            SourceProgramId = registration.ProgramId,
            TargetProgramId = rule.TargetProgramId,
            SourceRegistrationId = registration.Id,
            SourceEnrollmentId = sourceEnrollment?.Id,
            AssessmentDate = command.AssessmentDate ?? now,
            Method = rule.Method,
            Status = ProgramProgressionAssessmentStatus.Recorded,
            PassedInClass = command.PassedInClass,
            ListeningScore = command.ListeningScore,
            SpeakingScore = command.SpeakingScore,
            ReadingWritingScore = command.ReadingWritingScore,
            ReadingScore = command.ReadingScore,
            WritingScore = command.WritingScore,
            OverallScore = computed.OverallScore,
            ListeningShieldCount = computed.ListeningShieldCount,
            SpeakingShieldCount = computed.SpeakingShieldCount,
            ReadingWritingShieldCount = computed.ReadingWritingShieldCount,
            TotalShieldCount = computed.TotalShieldCount,
            IsEligible = computed.IsEligible,
            ResultBand = computed.ResultBand,
            ResultLevel = computed.ResultLevel,
            Comment = string.IsNullOrWhiteSpace(command.Comment) ? null : command.Comment.Trim(),
            AttachmentUrls = ProgramProgressionAttachmentUrlHelper.Serialize(command.AttachmentUrls),
            RecordedBy = userContext.UserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ProgramProgressionAssessments.Add(assessment);

        if (scheduleParticipant is not null)
        {
            scheduleParticipant.Status = ProgramProgressionScheduleParticipantStatus.Completed;
            scheduleParticipant.UpdatedAt = now;

            if (scheduleParticipant.Schedule.Participants.All(p =>
                    p.Id == scheduleParticipant.Id ||
                    p.Status != ProgramProgressionScheduleParticipantStatus.Scheduled))
            {
                scheduleParticipant.Schedule.Status = ProgramProgressionScheduleStatus.Completed;
                scheduleParticipant.Schedule.UpdatedAt = now;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var result = await ProgramProgressionAssessmentReadQuery.Build(context)
            .FirstAsync(a => a.Id == assessment.Id, cancellationToken);

        return Result.Success(result.ToDto());
    }
}
