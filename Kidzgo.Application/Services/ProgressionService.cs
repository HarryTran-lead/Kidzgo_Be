using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class ProgressionService(IDbContext context)
{
    public const decimal DefaultCompletionThreshold = 80m;

    public async Task<Result<StudentProgress>> UpsertStudentProgressAsync(
        Guid studentProfileId,
        Guid moduleId,
        Guid? currentLessonPlanTemplateId,
        decimal? completionPercentOverride,
        CancellationToken cancellationToken)
    {
        var module = await context.Modules
            .Include(x => x.Level)
            .FirstOrDefaultAsync(x => x.Id == moduleId, cancellationToken);

        if (module is null)
        {
            return Result.Failure<StudentProgress>(AcademicProgressionErrors.ModuleNotFound(moduleId));
        }

        var studentExists = await context.Profiles
            .AnyAsync(x => x.Id == studentProfileId && x.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);
        if (!studentExists)
        {
            return Result.Failure<StudentProgress>(
                Error.NotFound("AcademicProgression.StudentNotFound", $"Student '{studentProfileId}' was not found."));
        }

        var progress = await context.StudentProgresses
            .FirstOrDefaultAsync(x => x.StudentProfileId == studentProfileId && x.ModuleId == moduleId, cancellationToken);

        var now = VietnamTime.UtcNow();
        if (progress is null)
        {
            progress = new StudentProgress
            {
                Id = Guid.NewGuid(),
                StudentProfileId = studentProfileId,
                ModuleId = moduleId,
                Status = StudentProgressStatus.NotStarted,
                AssessmentStatus = StudentProgressAssessmentStatus.Pending,
                PromotionStatus = PromotionStatus.Pending,
                CompletionPercent = 0,
                StartedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.StudentProgresses.Add(progress);
        }

        var calculatedCompletion = completionPercentOverride ?? await CalculateCompletionPercentAsync(
            studentProfileId,
            moduleId,
            cancellationToken);

        progress.CompletionPercent = decimal.Round(Math.Clamp(calculatedCompletion, 0, 100), 2);
        progress.CurrentLessonPlanTemplateId = currentLessonPlanTemplateId;
        progress.UpdatedAt = now;
        progress.StartedAt ??= now;

        if (progress.CompletionPercent <= 0)
        {
            progress.Status = StudentProgressStatus.NotStarted;
        }
        else if (progress.PromotionStatus == PromotionStatus.RemedialRequired)
        {
            progress.Status = StudentProgressStatus.RemedialRequired;
        }
        else if (progress.CompletionPercent >= DefaultCompletionThreshold)
        {
            progress.Status = StudentProgressStatus.Completed;
            progress.CompletedAt ??= now;
        }
        else
        {
            progress.Status = StudentProgressStatus.InProgress;
            progress.CompletedAt = null;
        }

        return Result.Success(progress);
    }

    public async Task<decimal> CalculateCompletionPercentAsync(
        Guid studentProfileId,
        Guid moduleId,
        CancellationToken cancellationToken)
    {
        var templates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SessionOrder)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (templates.Count == 0)
        {
            return 0;
        }

        var lessonCompletions = await (
                from attendance in context.Attendances.AsNoTracking()
                join session in context.Sessions.AsNoTracking() on attendance.SessionId equals session.Id
                join lessonPlan in context.LessonPlans.AsNoTracking() on session.Id equals lessonPlan.SessionId
                where attendance.StudentProfileId == studentProfileId
                      && (attendance.AttendanceStatus == Domain.Sessions.AttendanceStatus.Present
                          || attendance.AttendanceStatus == Domain.Sessions.AttendanceStatus.Makeup)
                      && lessonPlan.TemplateId.HasValue
                      && templates.Contains(lessonPlan.TemplateId.Value)
                      && !lessonPlan.IsDeleted
                select new
                {
                    TemplateId = lessonPlan.TemplateId!.Value,
                    Completion = lessonPlan.CompletionPercent ?? 100m
                })
            .ToListAsync(cancellationToken);

        if (lessonCompletions.Count == 0)
        {
            return 0;
        }

        var completionByTemplate = lessonCompletions
            .GroupBy(x => x.TemplateId)
            .ToDictionary(x => x.Key, x => Math.Clamp(x.Max(i => i.Completion), 0, 100));

        decimal total = templates.Sum(templateId => completionByTemplate.GetValueOrDefault(templateId, 0m));
        return total / templates.Count;
    }

    public async Task<Module?> GetNextModuleAsync(Guid moduleId, CancellationToken cancellationToken)
    {
        var currentModule = await context.Modules
            .AsNoTracking()
            .Include(x => x.Level)
            .FirstOrDefaultAsync(x => x.Id == moduleId, cancellationToken);

        if (currentModule is null)
        {
            return null;
        }

        var nextModuleInLevel = await context.Modules
            .AsNoTracking()
            .Where(x => x.LevelId == currentModule.LevelId && x.Order > currentModule.Order && x.IsActive)
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextModuleInLevel is not null)
        {
            return nextModuleInLevel;
        }

        var nextLevel = await context.Levels
            .AsNoTracking()
            .Where(x => x.ProgramId == currentModule.Level.ProgramId && x.Order > currentModule.Level.Order && x.IsActive)
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextLevel is null)
        {
            return null;
        }

        return await context.Modules
            .AsNoTracking()
            .Where(x => x.LevelId == nextLevel.Id && x.IsActive)
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
