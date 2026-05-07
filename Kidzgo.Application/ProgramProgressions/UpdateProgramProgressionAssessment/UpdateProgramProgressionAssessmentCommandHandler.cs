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

        // Convert Practice Scores to Cambridge Scale if provided
        decimal? listeningScore = command.ListeningScore;
        decimal? speakingScore = command.SpeakingScore;
        decimal? readingScore = command.ReadingScore;
        decimal? writingScore = command.WritingScore;

        if (assessment.Rule.Method == ProgramProgressionMethod.CambridgeScale)
        {
            var practiceTestMappings = ProgramProgressionRuleDefinition.DeserializePracticeTestScoreMappings(assessment.Rule.PracticeTestScoreMappingsJson);

            if (command.ListeningPracticeScore.HasValue)
            {
                listeningScore = ProgramProgressionRuleDefinition.ConvertPracticeScoreToCambridgeScale(
                    command.ListeningPracticeScore.Value,
                    ProgramProgressionSkillType.Listening,
                    practiceTestMappings);
            }

            if (command.SpeakingPracticeScore.HasValue)
            {
                speakingScore = ProgramProgressionRuleDefinition.ConvertPracticeScoreToCambridgeScale(
                    command.SpeakingPracticeScore.Value,
                    ProgramProgressionSkillType.Speaking,
                    practiceTestMappings);
            }

            if (command.ReadingPracticeScore.HasValue)
            {
                readingScore = ProgramProgressionRuleDefinition.ConvertPracticeScoreToCambridgeScale(
                    command.ReadingPracticeScore.Value,
                    ProgramProgressionSkillType.Reading,
                    practiceTestMappings);
            }

            if (command.WritingPracticeScore.HasValue)
            {
                writingScore = ProgramProgressionRuleDefinition.ConvertPracticeScoreToCambridgeScale(
                    command.WritingPracticeScore.Value,
                    ProgramProgressionSkillType.Writing,
                    practiceTestMappings);
            }
        }

        var evaluationResult = evaluationService.Evaluate(
            assessment.Rule,
            new ProgramProgressionAssessmentInput(
                command.PassedInClass,
                listeningScore,
                speakingScore,
                command.ReadingWritingScore,
                readingScore,
                writingScore));
        if (evaluationResult.IsFailure)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(evaluationResult.Error);
        }

        var computed = evaluationResult.Value;
        assessment.AssessmentDate = command.AssessmentDate ?? assessment.AssessmentDate;
        assessment.PassedInClass = command.PassedInClass;
        assessment.ListeningPracticeScore = command.ListeningPracticeScore;
        assessment.SpeakingPracticeScore = command.SpeakingPracticeScore;
        assessment.ReadingPracticeScore = command.ReadingPracticeScore;
        assessment.WritingPracticeScore = command.WritingPracticeScore;
        assessment.ListeningScore = listeningScore;
        assessment.SpeakingScore = speakingScore;
        assessment.ReadingWritingScore = command.ReadingWritingScore;
        assessment.ReadingScore = readingScore;
        assessment.WritingScore = writingScore;
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
