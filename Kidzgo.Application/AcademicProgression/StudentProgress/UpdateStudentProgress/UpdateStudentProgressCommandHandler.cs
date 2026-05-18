using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.StudentProgress.UpdateStudentProgress;

public sealed class UpdateStudentProgressCommandHandler(
    IDbContext context,
    ProgressionService progressionService)
    : ICommandHandler<UpdateStudentProgressCommand, StudentProgressDto>
{
    public async Task<Result<StudentProgressDto>> Handle(UpdateStudentProgressCommand command, CancellationToken cancellationToken)
    {
        var result = await progressionService.UpsertStudentProgressAsync(
            command.StudentProfileId,
            command.ModuleId,
            command.CurrentLessonPlanTemplateId,
            command.CompletionPercent,
            cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<StudentProgressDto>(result.Error);
        }

        await context.SaveChangesAsync(cancellationToken);

        var progress = await context.StudentProgresses
            .AsNoTracking()
            .Include(x => x.Module)
            .ThenInclude(x => x.Level)
            .FirstAsync(x => x.Id == result.Value.Id, cancellationToken);

        return Result.Success(new StudentProgressDto
        {
            Id = progress.Id,
            StudentProfileId = progress.StudentProfileId,
            ModuleId = progress.ModuleId,
            ModuleCode = progress.Module.Code,
            ModuleName = progress.Module.Name,
            LevelCode = progress.Module.Level.Code,
            Status = progress.Status.ToString(),
            CompletionPercent = progress.CompletionPercent,
            AssessmentStatus = progress.AssessmentStatus.ToString(),
            PromotionStatus = progress.PromotionStatus.ToString(),
            LastAssessmentId = progress.LastAssessmentId,
            CurrentLessonPlanTemplateId = progress.CurrentLessonPlanTemplateId,
            StartedAt = progress.StartedAt,
            CompletedAt = progress.CompletedAt
        });
    }
}
