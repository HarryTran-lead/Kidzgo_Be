using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.GetHomeworkAssignments;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.GetMyCreatedHomeworkAssignments;

public sealed class GetMyCreatedHomeworkAssignmentsQuery : IQuery<GetHomeworkAssignmentsResponse>, IPageableQuery
{
    public Guid? ClassId { get; init; }
    public Guid? SessionId { get; init; }
    public string? Skill { get; init; }
    public SubmissionType? SubmissionType { get; init; }
    public Guid? BranchId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
