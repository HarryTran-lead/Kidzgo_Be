using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.GetClassAcademicDashboard;

public sealed class GetClassAcademicDashboardQuery : IQuery<ClassAcademicDashboardResponse>
{
    public Guid ClassId { get; init; }
    public Guid? PeriodId { get; init; }
}
