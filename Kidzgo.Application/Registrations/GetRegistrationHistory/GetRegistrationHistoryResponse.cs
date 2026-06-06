using Kidzgo.Application.AuditLogs.GetAuditLogs;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetRegistrationHistory;

public sealed class GetRegistrationHistoryResponse
{
    public Guid RegistrationId { get; init; }
    public Page<AuditLogDto> History { get; init; } = null!;
}
