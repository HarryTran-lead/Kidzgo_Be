using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Registrations.GetRegistrationHistory;

public sealed class GetRegistrationHistoryQuery : IQuery<GetRegistrationHistoryResponse>, IPageableQuery
{
    public Guid RegistrationId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
