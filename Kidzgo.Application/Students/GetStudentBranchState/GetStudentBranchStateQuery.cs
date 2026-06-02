using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;

namespace Kidzgo.Application.Students.GetStudentBranchState;

public sealed class GetStudentBranchStateQuery : IQuery<StudentBranchStateDto>
{
    public Guid StudentProfileId { get; init; }
}
