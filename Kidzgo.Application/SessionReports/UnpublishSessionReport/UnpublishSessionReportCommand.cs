using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.UnpublishSessionReport;

public sealed record UnpublishSessionReportCommand(Guid SessionReportId) : ICommand<UnpublishSessionReportResponse>;
