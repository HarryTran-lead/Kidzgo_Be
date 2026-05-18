using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class AssessmentService(
    IDbContext context,
    ProgressionService progressionService)
{
    public const decimal DefaultPassScore = 70m;

    public async Task<Result<Assessment>> CreateAssessmentAsync(
        Guid studentProfileId,
        Guid moduleId,
        Guid? sessionId,
        string type,
        decimal score,
        string? teacherComment,
        Guid assessedBy,
        DateTime? assessedAt,
        CancellationToken cancellationToken)
    {
        var moduleExists = await context.Modules.AnyAsync(x => x.Id == moduleId, cancellationToken);
        if (!moduleExists)
        {
            return Result.Failure<Assessment>(AcademicProgressionErrors.ModuleNotFound(moduleId));
        }

        var studentExists = await context.Profiles
            .AnyAsync(x => x.Id == studentProfileId && x.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);
        if (!studentExists)
        {
            return Result.Failure<Assessment>(
                Error.NotFound("AcademicProgression.StudentNotFound", $"Student '{studentProfileId}' was not found."));
        }

        if (sessionId.HasValue)
        {
            var sessionExists = await context.Sessions.AnyAsync(x => x.Id == sessionId.Value, cancellationToken);
            if (!sessionExists)
            {
                return Result.Failure<Assessment>(
                    Error.NotFound("AcademicProgression.SessionNotFound", $"Session '{sessionId}' was not found."));
            }
        }

        var now = VietnamTime.UtcNow();
        var assessment = new Assessment
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            ModuleId = moduleId,
            SessionId = sessionId,
            Type = type.Trim(),
            Score = score,
            Result = score >= DefaultPassScore ? AssessmentResult.Pass : AssessmentResult.Fail,
            TeacherComment = string.IsNullOrWhiteSpace(teacherComment) ? null : teacherComment.Trim(),
            AssessedBy = assessedBy,
            AssessedAt = assessedAt ?? now,
            CreatedAt = now
        };

        context.Assessments.Add(assessment);

        var progressResult = await progressionService.UpsertStudentProgressAsync(
            studentProfileId,
            moduleId,
            null,
            null,
            cancellationToken);

        if (progressResult.IsFailure)
        {
            return Result.Failure<Assessment>(progressResult.Error);
        }

        var progress = progressResult.Value;
        progress.LastAssessmentId = assessment.Id;
        progress.AssessmentStatus = assessment.Result == AssessmentResult.Pass
            ? StudentProgressAssessmentStatus.Passed
            : StudentProgressAssessmentStatus.Failed;
        progress.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(assessment);
    }
}
