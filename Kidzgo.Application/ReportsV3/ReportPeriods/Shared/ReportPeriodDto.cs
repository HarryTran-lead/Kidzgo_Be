namespace Kidzgo.Application.ReportsV3.ReportPeriods.Shared;

public sealed class ReportPeriodDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateTime CreatedAt { get; init; }
}
