namespace Kidzgo.Application.Enrollments.BackfillStudentSessionAssignments;

public sealed class BackfillStudentSessionAssignmentsResponse
{
    public int MatchedEnrollments { get; init; }
    public int ProcessedEnrollments { get; init; }
    public int AffectedClasses { get; init; }
    public int BatchSize { get; init; }
    public int CreatedAssignments { get; init; }
    public int ReactivatedAssignments { get; init; }
    public int CancelledAssignments { get; init; }
}
