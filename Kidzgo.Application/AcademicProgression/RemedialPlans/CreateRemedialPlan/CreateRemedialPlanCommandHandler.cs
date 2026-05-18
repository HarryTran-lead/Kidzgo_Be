using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.RemedialPlans.CreateRemedialPlan;

public sealed class CreateRemedialPlanCommandHandler(
    IDbContext context,
    IUserContext userContext,
    RemedialService remedialService)
    : ICommandHandler<CreateRemedialPlanCommand, RemedialPlanDto>
{
    public async Task<Result<RemedialPlanDto>> Handle(CreateRemedialPlanCommand command, CancellationToken cancellationToken)
    {
        var result = await remedialService.CreateRemedialPlanAsync(
            command.StudentProfileId,
            command.ModuleId,
            command.WeakSkills,
            command.RecommendedSessionCount,
            command.Notes,
            userContext.UserId,
            cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<RemedialPlanDto>(result.Error);
        }

        var plan = await context.RemedialPlans
            .AsNoTracking()
            .Include(x => x.Module)
            .FirstAsync(x => x.Id == result.Value.Id, cancellationToken);

        return Result.Success(new RemedialPlanDto
        {
            Id = plan.Id,
            StudentProfileId = plan.StudentProfileId,
            ModuleId = plan.ModuleId,
            ModuleCode = plan.Module.Code,
            WeakSkills = plan.WeakSkills,
            RecommendedSessionCount = plan.RecommendedSessionCount,
            Notes = plan.Notes,
            CreatedBy = plan.CreatedBy,
            CreatedAt = plan.CreatedAt
        });
    }
}
