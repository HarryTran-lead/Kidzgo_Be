using Kidzgo.Application.ReportsV3.Shared;

namespace Kidzgo.Application.ReportsV3.GetClassAcademicDashboard;

public sealed class ClassAcademicDashboardResponse
{
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public int TotalStudents { get; init; }
    public int WeakStudents { get; init; }
    public int DelayedStudents { get; init; }
    public int FailedAssessments { get; init; }
    public int RemedialRequired { get; init; }
    public ClassPacingDto ClassPacing { get; init; } = new();
    public IReadOnlyCollection<RiskAlertDto> RiskAlerts { get; init; } = Array.Empty<RiskAlertDto>();
}

public sealed class ClassPacingDto
{
    public decimal ReviewRatioPercent { get; init; }
    public decimal PlannedProgressPercent { get; init; }
    public decimal ActualProgressPercent { get; init; }
    public bool CurriculumDelayRisk { get; init; }
}
