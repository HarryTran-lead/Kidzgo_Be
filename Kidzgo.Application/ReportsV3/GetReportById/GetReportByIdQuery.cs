using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;

namespace Kidzgo.Application.ReportsV3.GetReportById;

public sealed class GetReportByIdQuery : IQuery<StudentReportDetailDto>
{
    public Guid ReportId { get; init; }
}
