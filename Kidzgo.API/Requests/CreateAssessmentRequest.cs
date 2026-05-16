namespace Kidzgo.API.Requests;

public sealed class CreateAssessmentRequest
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public Guid? SessionId { get; init; }
    public string Type { get; init; } = null!;
    public decimal Score { get; init; }
    public string? TeacherComment { get; init; }
    public DateTime? AssessedAt { get; init; }
}
