using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.BackfillStudentSessionAssignments;

public sealed class BackfillStudentSessionAssignmentsCommand : ICommand<BackfillStudentSessionAssignmentsResponse>
{
    public Guid? EnrollmentId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public int? BatchSize { get; init; }
}
