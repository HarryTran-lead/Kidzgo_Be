using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.GetLatestStudentReport;

public sealed class GetLatestStudentReportQuery : IQuery<StudentReportDetailDto>
{
    public Guid StudentId { get; init; }
    public StudentReportType? ReportType { get; init; }
}
