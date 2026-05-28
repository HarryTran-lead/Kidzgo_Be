using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class RiskRuleConfig : Entity
{
    public Guid Id { get; set; }
    public RiskType RiskType { get; set; }
    public int Score { get; set; }
    public bool IsActive { get; set; } = true;
    public string ParametersJson { get; set; } = "{}";
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? UpdatedByUser { get; set; }
}
