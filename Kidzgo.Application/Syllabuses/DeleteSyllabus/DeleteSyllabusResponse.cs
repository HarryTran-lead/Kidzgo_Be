namespace Kidzgo.Application.Syllabuses.DeleteSyllabus;

public sealed class DeleteSyllabusResponse
{
    public Guid Id { get; init; }
    public int DeletedLessonPlanTemplateCount { get; init; }
    public int DeletedLessonPlanUnitCount { get; init; }
}
