using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.AcademicProgression.StudentProgress.GetStudentProgress;

public sealed class GetStudentProgressQuery : IQuery<GetStudentProgressResponse>
{
    public Guid StudentProfileId { get; init; }
}
