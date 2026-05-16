using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.StudentProgress.GetStudentProgress;

public sealed class GetStudentProgressQueryHandler(IDbContext context)
    : IQueryHandler<GetStudentProgressQuery, GetStudentProgressResponse>
{
    public async Task<Result<GetStudentProgressResponse>> Handle(GetStudentProgressQuery query, CancellationToken cancellationToken)
    {
        var items = await context.StudentProgresses
            .AsNoTracking()
            .Include(x => x.Module)
            .ThenInclude(x => x.Level)
            .Where(x => x.StudentProfileId == query.StudentProfileId)
            .OrderBy(x => x.Module.Level.Order)
            .ThenBy(x => x.Module.Order)
            .Select(x => new StudentProgressDto
            {
                Id = x.Id,
                StudentProfileId = x.StudentProfileId,
                ModuleId = x.ModuleId,
                ModuleCode = x.Module.Code,
                ModuleName = x.Module.Name,
                LevelCode = x.Module.Level.Code,
                Status = x.Status.ToString(),
                CompletionPercent = x.CompletionPercent,
                AssessmentStatus = x.AssessmentStatus.ToString(),
                PromotionStatus = x.PromotionStatus.ToString(),
                LastAssessmentId = x.LastAssessmentId,
                CurrentLessonPlanTemplateId = x.CurrentLessonPlanTemplateId,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetStudentProgressResponse { Items = items });
    }
}
