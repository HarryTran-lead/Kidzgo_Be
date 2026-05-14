using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.UpdateSessionSectionType;

public sealed class UpdateSessionSectionTypeCommand : ICommand<UpdateSessionSectionTypeResponse>
{
    public Guid SessionId { get; init; }
    public SectionType SectionType { get; init; }
    public bool IsPrivilegedUser { get; init; }
}
