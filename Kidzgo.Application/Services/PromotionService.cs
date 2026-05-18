using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class PromotionService(
    IDbContext context,
    ProgressionService progressionService,
    RemedialService remedialService)
{
    public async Task<Result<PromotionDecision>> EvaluateAndCreateDecisionAsync(
        Guid studentProfileId,
        Guid moduleId,
        string? reason,
        Guid approvedBy,
        DateTime? approvedAt,
        CancellationToken cancellationToken)
    {
        var progressResult = await progressionService.UpsertStudentProgressAsync(
            studentProfileId,
            moduleId,
            null,
            null,
            cancellationToken);
        if (progressResult.IsFailure)
        {
            return Result.Failure<PromotionDecision>(progressResult.Error);
        }

        var progress = progressResult.Value;
        var latestAssessment = await context.Assessments
            .AsNoTracking()
            .Where(x => x.StudentProfileId == studentProfileId && x.ModuleId == moduleId)
            .OrderByDescending(x => x.AssessedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var latestEvaluation = await context.TeacherEvaluations
            .AsNoTracking()
            .Where(x => x.StudentProfileId == studentProfileId && x.ModuleId == moduleId)
            .OrderByDescending(x => x.EvaluatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var decisionValue = DetermineDecision(progress, latestAssessment, latestEvaluation);
        var now = approvedAt ?? VietnamTime.UtcNow();

        var decision = new PromotionDecision
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            ModuleId = moduleId,
            Decision = decisionValue,
            Reason = string.IsNullOrWhiteSpace(reason)
                ? BuildDecisionReason(progress, latestAssessment, latestEvaluation, decisionValue)
                : reason.Trim(),
            ApprovedBy = approvedBy,
            ApprovedAt = now,
            CreatedAt = now
        };

        context.PromotionDecisions.Add(decision);

        progress.PromotionStatus = decisionValue switch
        {
            PromotionDecisionResult.Pass => PromotionStatus.Passed,
            PromotionDecisionResult.Fail => PromotionStatus.Failed,
            _ => PromotionStatus.RemedialRequired
        };
        progress.Status = decisionValue switch
        {
            PromotionDecisionResult.Pass => StudentProgressStatus.Completed,
            PromotionDecisionResult.Fail => StudentProgressStatus.InProgress,
            _ => StudentProgressStatus.RemedialRequired
        };
        progress.CompletedAt = decisionValue == PromotionDecisionResult.Pass ? now : null;
        progress.UpdatedAt = now;

        if (decisionValue == PromotionDecisionResult.Pass)
        {
            var nextModule = await progressionService.GetNextModuleAsync(moduleId, cancellationToken);
            if (nextModule is not null)
            {
                await progressionService.UpsertStudentProgressAsync(
                    studentProfileId,
                    nextModule.Id,
                    null,
                    0,
                    cancellationToken);
            }
        }
        else if (decisionValue == PromotionDecisionResult.RemedialRequired)
        {
            var weakSkills = BuildWeakSkills(latestEvaluation);
            var remedialResult = await remedialService.CreateRemedialPlanAsync(
                studentProfileId,
                moduleId,
                weakSkills,
                2,
                decision.Reason,
                approvedBy,
                cancellationToken);
            if (remedialResult.IsFailure)
            {
                return Result.Failure<PromotionDecision>(remedialResult.Error);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(decision);
    }

    private static PromotionDecisionResult DetermineDecision(
        StudentProgress progress,
        Assessment? assessment,
        TeacherEvaluation? evaluation)
    {
        if (assessment?.Result == AssessmentResult.Fail)
        {
            return PromotionDecisionResult.RemedialRequired;
        }

        if (assessment?.Result == AssessmentResult.Pass
            && progress.CompletionPercent >= ProgressionService.DefaultCompletionThreshold
            && (evaluation?.Confidence ?? 0) >= 3)
        {
            return PromotionDecisionResult.Pass;
        }

        if ((evaluation?.Confidence ?? 0) < 3 || (evaluation?.Speaking ?? 0) < 3)
        {
            return PromotionDecisionResult.RemedialRequired;
        }

        return PromotionDecisionResult.Fail;
    }

    private static string BuildDecisionReason(
        StudentProgress progress,
        Assessment? assessment,
        TeacherEvaluation? evaluation,
        PromotionDecisionResult decision)
    {
        return decision switch
        {
            PromotionDecisionResult.Pass =>
                $"Completion {progress.CompletionPercent:0.##}%, assessment pass, confidence {(evaluation?.Confidence ?? 0)}/5.",
            PromotionDecisionResult.RemedialRequired =>
                $"Completion {progress.CompletionPercent:0.##}%, assessment {assessment?.Result}, weak skills: {BuildWeakSkills(evaluation)}.",
            _ =>
                $"Completion {progress.CompletionPercent:0.##}% is not enough for promotion."
        };
    }

    private static string BuildWeakSkills(TeacherEvaluation? evaluation)
    {
        if (evaluation is null)
        {
            return "general reinforcement";
        }

        var weakSkills = new List<string>();
        if (evaluation.Speaking < 3) weakSkills.Add("speaking");
        if (evaluation.Listening < 3) weakSkills.Add("listening");
        if (evaluation.Reading < 3) weakSkills.Add("reading");
        if (evaluation.Writing < 3) weakSkills.Add("writing");
        if (evaluation.Confidence < 3) weakSkills.Add("confidence");

        return weakSkills.Count == 0 ? "general reinforcement" : string.Join(", ", weakSkills);
    }
}
