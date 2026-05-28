namespace Kidzgo.API.Requests;

public sealed class UpdateReportPeriodRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "monthly";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
