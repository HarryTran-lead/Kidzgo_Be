namespace Kidzgo.Application.ReportsV3.RiskRuleConfigs;

public sealed class RiskRuleConfigDto
{
    public string RiskType { get; init; } = string.Empty;
    public int Score { get; init; }
    public bool IsActive { get; init; }
    public string ParametersJson { get; init; } = "{}";
    public bool IsCustomized { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
