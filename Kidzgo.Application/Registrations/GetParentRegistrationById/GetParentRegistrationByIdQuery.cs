using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.GetRegistrationById;

namespace Kidzgo.Application.Registrations.GetParentRegistrationById;

public sealed class GetParentRegistrationByIdQuery : IQuery<GetRegistrationByIdResponse>
{
    public Guid Id { get; init; }
}
