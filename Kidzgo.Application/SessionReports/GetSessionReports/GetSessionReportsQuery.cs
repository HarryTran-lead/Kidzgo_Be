using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.SessionReports.GetSessionReports;

public sealed class GetSessionReportsQuery : IQuery<GetSessionReportsResponse>, IPageableQuery
{
    public Guid? SessionId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? TeacherUserId { get; init; }
    public Guid? ClassId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public ReportStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
