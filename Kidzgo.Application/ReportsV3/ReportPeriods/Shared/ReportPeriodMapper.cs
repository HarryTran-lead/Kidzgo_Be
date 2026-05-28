using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.Shared;

internal static class ReportPeriodMapper
{
    public static ReportPeriodDto ToDto(ReportPeriod period)
    {
        return new ReportPeriodDto
        {
            Id = period.Id,
            Code = period.Code,
            Name = period.Name,
            Type = period.Type.ToString(),
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            CreatedAt = period.CreatedAt
        };
    }
}
