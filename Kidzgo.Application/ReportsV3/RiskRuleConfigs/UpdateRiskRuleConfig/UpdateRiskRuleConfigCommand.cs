using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.RiskRuleConfigs;

public sealed class UpdateRiskRuleConfigCommand : ICommand<RiskRuleConfigDto>
{
    public RiskType RiskType { get; init; }
    public int Score { get; init; }
    public bool IsActive { get; init; }
    public string? ParametersJson { get; init; }
}
