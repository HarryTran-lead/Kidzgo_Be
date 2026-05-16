using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.Assessments.CreateAssessment;

public sealed class CreateAssessmentCommand : ICommand<AssessmentDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public Guid? SessionId { get; init; }
    public string Type { get; init; } = null!;
    public decimal Score { get; init; }
    public string? TeacherComment { get; init; }
    public DateTime? AssessedAt { get; init; }
}
