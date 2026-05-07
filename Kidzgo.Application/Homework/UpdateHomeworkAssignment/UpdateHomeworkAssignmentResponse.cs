namespace Kidzgo.Application.Homework.UpdateHomeworkAssignment;

public sealed class UpdateHomeworkAssignmentResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public DateTime? StartDate { get; init; }
    public DateTime? DueAt { get; init; }
}

