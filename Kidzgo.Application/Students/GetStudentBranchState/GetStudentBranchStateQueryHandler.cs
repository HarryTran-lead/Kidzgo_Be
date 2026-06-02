using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;

namespace Kidzgo.Application.Students.GetStudentBranchState;

public sealed class GetStudentBranchStateQueryHandler(IDbContext context)
    : IQueryHandler<GetStudentBranchStateQuery, StudentBranchStateDto>
{
    public Task<Domain.Common.Result<StudentBranchStateDto>> Handle(
        GetStudentBranchStateQuery query,
        CancellationToken cancellationToken)
    {
        return StudentBranchReadModelBuilder.BuildAsync(context, query.StudentProfileId, cancellationToken);
    }
}
