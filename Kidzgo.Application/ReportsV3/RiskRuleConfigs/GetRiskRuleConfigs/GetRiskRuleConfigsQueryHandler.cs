using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.RiskRuleConfigs;

public sealed class GetRiskRuleConfigsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetRiskRuleConfigsQuery, IReadOnlyCollection<RiskRuleConfigDto>>
{
    public async Task<Result<IReadOnlyCollection<RiskRuleConfigDto>>> Handle(
        GetRiskRuleConfigsQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<IReadOnlyCollection<RiskRuleConfigDto>>(
                Error.NotFound("Report.UserNotFound", "Current user was not found."));
        }

        if (currentUser.Role != UserRole.Admin)
        {
            return Result.Failure<IReadOnlyCollection<RiskRuleConfigDto>>(
                Error.Unauthorized("Report.AccessDenied", "Only admin can manage risk rule configs."));
        }

        var configs = await context.RiskRuleConfigs
            .ToListAsync(cancellationToken);

        var configByRiskType = configs.ToDictionary(x => x.RiskType, x => x);

        var response = Enum.GetValues<RiskType>()
            .Select(riskType =>
            {
                if (configByRiskType.TryGetValue(riskType, out var config))
                {
                    return new RiskRuleConfigDto
                    {
                        RiskType = riskType.ToString(),
                        Score = config.Score,
                        IsActive = config.IsActive,
                        ParametersJson = string.IsNullOrWhiteSpace(config.ParametersJson)
                            ? RiskRuleDefaults.GetDefaultParametersJson(riskType)
                            : config.ParametersJson,
                        IsCustomized = true,
                        UpdatedBy = config.UpdatedBy,
                        UpdatedAt = config.UpdatedAt
                    };
                }

                return new RiskRuleConfigDto
                {
                    RiskType = riskType.ToString(),
                    Score = RiskRuleDefaults.GetDefaultScore(riskType),
                    IsActive = true,
                    ParametersJson = RiskRuleDefaults.GetDefaultParametersJson(riskType),
                    IsCustomized = false
                };
            })
            .ToList();

        return Result.Success<IReadOnlyCollection<RiskRuleConfigDto>>(response);
    }
}
