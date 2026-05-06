using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionAssessment;

public sealed class UpdateProgramProgressionAssessmentCommandHandler(
    IDbContext context,
    Kidzgo.Application.Abstraction.Authentication.IUserContext userContext,
    ProgramProgressionEvaluationService evaluationService)
    : ICommandHandler<UpdateProgramProgressionAssessmentCommand, ProgramProgressionAssessmentDto>
{
    public async Task<Result<ProgramProgressionAssessmentDto>> Handle(
        UpdateProgramProgressionAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        var currentUserRole = await ProgramProgressionAccessHelper.GetCurrentUserRoleAsync(
            context,
            userContext.UserId,
            cancellationToken);

        var assessment = await context.ProgramProgressionAssessments
            .Include(a => a.Rule)
            .Include(a => a.ScheduleParticipant)
                .ThenInclude(participant => participant!.Schedule)
            .Include(a => a.SourceEnrollment)
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(
                ProgramProgressionErrors.AssessmentNotFound(command.Id));
        }

        if (assessment.Status == ProgramProgressionAssessmentStatus.Approved)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(
                ProgramProgressionErrors.AssessmentAlreadyApproved(command.Id));
        }

        if (currentUserRole == Domain.Users.UserRole.Teacher)
        {
            Result teacherAccess;

            if (assessment.ScheduleParticipantId.HasValue && assessment.ScheduleParticipant is not null)
            {
                teacherAccess = await ProgramProgressionAccessHelper.EnsureTeacherAssignedToScheduleAsync(
                    context,
                    userContext.UserId,
                    assessment.ScheduleParticipant.ScheduleId,
                    cancellationToken);
            }
            else if (assessment.SourceEnrollment?.ClassId is Guid sourceClassId)
            {
                teacherAccess = await ProgramProgressionAccessHelper.EnsureTeacherCanManageClassAssessmentAsync(
                    context,
                    userContext.UserId,
                    sourceClassId,
                    cancellationToken);
            }
            else
            {
                teacherAccess = Result.Failure(
                    ProgramProgressionErrors.TeacherCannotManageAssessment(userContext.UserId, Guid.Empty));
            }

            if (teacherAccess.IsFailure)
            {
                return Result.Failure<ProgramProgressionAssessmentDto>(teacherAccess.Error);
            }
        }

        var evaluationResult = evaluationService.Evaluate(
            assessment.Rule,
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

        var computed = evaluationResult.Value;
        assessment.AssessmentDate = command.AssessmentDate ?? assessment.AssessmentDate;
        assessment.PassedInClass = command.PassedInClass;
        assessment.ListeningScore = command.ListeningScore;
        assessment.SpeakingScore = command.SpeakingScore;
        assessment.ReadingWritingScore = command.ReadingWritingScore;
        assessment.ReadingScore = command.ReadingScore;
        assessment.WritingScore = command.WritingScore;
        assessment.OverallScore = computed.OverallScore;
        assessment.ListeningShieldCount = computed.ListeningShieldCount;
        assessment.SpeakingShieldCount = computed.SpeakingShieldCount;
        assessment.ReadingWritingShieldCount = computed.ReadingWritingShieldCount;
        assessment.TotalShieldCount = computed.TotalShieldCount;
        assessment.IsEligible = computed.IsEligible;
        assessment.ResultBand = computed.ResultBand;
        assessment.ResultLevel = computed.ResultLevel;
        assessment.Comment = string.IsNullOrWhiteSpace(command.Comment) ? null : command.Comment.Trim();
        assessment.AttachmentUrls = ProgramProgressionAttachmentUrlHelper.Serialize(command.AttachmentUrls);
        assessment.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        var result = await ProgramProgressionAssessmentReadQuery.Build(context)
            .FirstAsync(a => a.Id == assessment.Id, cancellationToken);

        return Result.Success(result.ToDto());
    }
}
