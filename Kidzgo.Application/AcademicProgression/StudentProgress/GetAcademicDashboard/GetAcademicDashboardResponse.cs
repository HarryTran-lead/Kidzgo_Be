namespace Kidzgo.Application.AcademicProgression.StudentProgress.GetAcademicDashboard;

public sealed class GetAcademicDashboardResponse
{
    public int InProgressStudents { get; init; }
    public int CompletedStudents { get; init; }
    public int RemedialRequiredStudents { get; init; }
    public int FailedPromotions { get; init; }
    public List<WeakModuleDto> WeakModules { get; init; } = [];
}

public sealed class WeakModuleDto
{
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public int RemedialCount { get; init; }
    public decimal AverageCompletionPercent { get; init; }
}
