using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.CreatePackageCurriculumMapping;

public sealed class CreatePackageCurriculumMappingCommand : ICommand<CreatePackageCurriculumMappingResponse>
{
    public Guid TuitionPlanId { get; init; }
    public Guid SyllabusId { get; init; }
}

public sealed class CreatePackageCurriculumMappingResponse
{
    public Guid Id { get; init; }
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public Guid SyllabusId { get; init; }
    public string SyllabusCode { get; init; } = null!;
    public string SyllabusVersion { get; init; } = null!;
    public string SyllabusTitle { get; init; } = null!;
    public bool IsActive { get; init; }
}
