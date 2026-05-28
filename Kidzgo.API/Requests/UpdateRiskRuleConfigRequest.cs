namespace Kidzgo.API.Requests;

public sealed class UpdateRiskRuleConfigRequest
{
    public bool IsActive { get; set; }
    public int Score { get; set; }
    public string? ParametersJson { get; set; } = "{}";
}
