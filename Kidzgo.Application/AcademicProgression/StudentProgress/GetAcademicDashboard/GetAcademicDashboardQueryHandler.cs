using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.StudentProgress.GetAcademicDashboard;

public sealed class GetAcademicDashboardQueryHandler(IDbContext context)
    : IQueryHandler<GetAcademicDashboardQuery, GetAcademicDashboardResponse>
{
    public async Task<Result<GetAcademicDashboardResponse>> Handle(GetAcademicDashboardQuery query, CancellationToken cancellationToken)
    {
        var progresses = context.StudentProgresses.AsNoTracking();

        var weakModules = await progresses
            .Include(x => x.Module)
            .GroupBy(x => new { x.ModuleId, x.Module.Code, x.Module.Name })
            .Select(x => new WeakModuleDto
            {
                ModuleId = x.Key.ModuleId,
                ModuleCode = x.Key.Code,
                ModuleName = x.Key.Name,
                RemedialCount = x.Count(i => i.PromotionStatus == Domain.AcademicProgression.PromotionStatus.RemedialRequired),
                AverageCompletionPercent = x.Average(i => i.CompletionPercent)
            })
            .OrderByDescending(x => x.RemedialCount)
            .ThenBy(x => x.AverageCompletionPercent)
            .Take(10)
            .ToListAsync(cancellationToken);

        return Result.Success(new GetAcademicDashboardResponse
        {
            InProgressStudents = await progresses.CountAsync(x => x.Status == Domain.AcademicProgression.StudentProgressStatus.InProgress, cancellationToken),
            CompletedStudents = await progresses.CountAsync(x => x.Status == Domain.AcademicProgression.StudentProgressStatus.Completed, cancellationToken),
            RemedialRequiredStudents = await progresses.CountAsync(x => x.Status == Domain.AcademicProgression.StudentProgressStatus.RemedialRequired, cancellationToken),
            FailedPromotions = await progresses.CountAsync(x => x.PromotionStatus == Domain.AcademicProgression.PromotionStatus.Failed, cancellationToken),
            WeakModules = weakModules
        });
    }
}
