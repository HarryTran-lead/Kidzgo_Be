using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.RiskRuleConfigs;

public sealed class UpdateRiskRuleConfigCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateRiskRuleConfigCommand, RiskRuleConfigDto>
{
    public async Task<Result<RiskRuleConfigDto>> Handle(
        UpdateRiskRuleConfigCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<RiskRuleConfigDto>(
                Error.NotFound("Report.UserNotFound", "Current user was not found."));
        }

        if (currentUser.Role != UserRole.Admin)
        {
            return Result.Failure<RiskRuleConfigDto>(
                Error.Unauthorized("Report.AccessDenied", "Only admin can manage risk rule configs."));
        }

        var now = VietnamTime.UtcNow();
        var normalizedParametersJson = NormalizeParametersJson(command.ParametersJson, command.RiskType);
        var config = await context.RiskRuleConfigs
            .FirstOrDefaultAsync(x => x.RiskType == command.RiskType, cancellationToken);

        if (config is null)
        {
            config = new RiskRuleConfig
            {
                Id = Guid.NewGuid(),
                RiskType = command.RiskType,
                Score = command.Score,
                IsActive = command.IsActive,
                ParametersJson = normalizedParametersJson,
                UpdatedBy = currentUser.Id,
                UpdatedAt = now
            };

            context.RiskRuleConfigs.Add(config);
        }
        else
        {
            config.Score = command.Score;
            config.IsActive = command.IsActive;
            config.ParametersJson = normalizedParametersJson;
            config.UpdatedBy = currentUser.Id;
            config.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new RiskRuleConfigDto
        {
            RiskType = config.RiskType.ToString(),
            Score = config.Score,
            IsActive = config.IsActive,
            ParametersJson = config.ParametersJson,
            IsCustomized = true,
            UpdatedBy = config.UpdatedBy,
            UpdatedAt = config.UpdatedAt
        });
    }

    private static string NormalizeParametersJson(string? parametersJson, RiskType riskType)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return RiskRuleDefaults.GetDefaultParametersJson(riskType);
        }

        return parametersJson.Trim();
    }
}
