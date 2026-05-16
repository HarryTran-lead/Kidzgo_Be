using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class RemedialService(IDbContext context)
{
    public async Task<Result<RemedialPlan>> CreateRemedialPlanAsync(
        Guid studentProfileId,
        Guid moduleId,
        string weakSkills,
        int recommendedSessionCount,
        string? notes,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        var moduleExists = await context.Modules.AnyAsync(x => x.Id == moduleId, cancellationToken);
        if (!moduleExists)
        {
            return Result.Failure<RemedialPlan>(AcademicProgressionErrors.ModuleNotFound(moduleId));
        }

        var studentExists = await context.Profiles
            .AnyAsync(x => x.Id == studentProfileId && x.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);
        if (!studentExists)
        {
            return Result.Failure<RemedialPlan>(
                Error.NotFound("AcademicProgression.StudentNotFound", $"Student '{studentProfileId}' was not found."));
        }

        var plan = new RemedialPlan
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            ModuleId = moduleId,
            WeakSkills = weakSkills.Trim(),
            RecommendedSessionCount = recommendedSessionCount,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedBy = createdBy,
            CreatedAt = VietnamTime.UtcNow()
        };

        context.RemedialPlans.Add(plan);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(plan);
    }
}
