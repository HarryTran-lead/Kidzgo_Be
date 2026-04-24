using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.GetClasses;

public sealed class GetClassesQuery : IQuery<GetClassesResponse>, IPageableQuery
{
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public Guid? TeacherId { get; init; }
    public ClassStatus? Status { get; init; }
    public Guid? StudentId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

