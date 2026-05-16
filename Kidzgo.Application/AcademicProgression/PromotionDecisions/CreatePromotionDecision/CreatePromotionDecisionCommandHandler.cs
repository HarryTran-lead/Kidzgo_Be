using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.PromotionDecisions.CreatePromotionDecision;

public sealed class CreatePromotionDecisionCommandHandler(
    IDbContext context,
    IUserContext userContext,
    PromotionService promotionService)
    : ICommandHandler<CreatePromotionDecisionCommand, PromotionDecisionDto>
{
    public async Task<Result<PromotionDecisionDto>> Handle(CreatePromotionDecisionCommand command, CancellationToken cancellationToken)
    {
        var result = await promotionService.EvaluateAndCreateDecisionAsync(
            command.StudentProfileId,
            command.ModuleId,
            command.Reason,
            userContext.UserId,
            command.ApprovedAt,
            cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<PromotionDecisionDto>(result.Error);
        }

        var decision = result.Value;
        var moduleCode = await context.Modules
            .AsNoTracking()
            .Where(x => x.Id == decision.ModuleId)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new PromotionDecisionDto
        {
            Id = decision.Id,
            StudentProfileId = decision.StudentProfileId,
            ModuleId = decision.ModuleId,
            ModuleCode = moduleCode ?? string.Empty,
            Decision = decision.Decision.ToString(),
            Reason = decision.Reason,
            ApprovedBy = decision.ApprovedBy,
            ApprovedAt = decision.ApprovedAt
        });
    }
}
