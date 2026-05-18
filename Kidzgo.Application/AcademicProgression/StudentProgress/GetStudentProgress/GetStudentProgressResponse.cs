using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.StudentProgress.GetStudentProgress;

public sealed class GetStudentProgressResponse
{
    public List<StudentProgressDto> Items { get; init; } = [];
}
