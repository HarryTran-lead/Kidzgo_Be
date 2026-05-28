using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.RiskRuleConfigs;

public sealed class GetRiskRuleConfigsQuery : IQuery<IReadOnlyCollection<RiskRuleConfigDto>>;
