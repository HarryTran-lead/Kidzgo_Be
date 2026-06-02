using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.GetPackageSyllabuses;

public sealed class GetPackageSyllabusesQuery : IQuery<GetPackageSyllabusesResponse>
{
    public Guid TuitionPlanId { get; init; }
}

public sealed class GetPackageSyllabusesResponse
{
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public IReadOnlyList<PackageSyllabusDto> Syllabuses { get; init; } = Array.Empty<PackageSyllabusDto>();
}

public sealed class PackageSyllabusDto
{
    public Guid MappingId { get; init; }
    public Guid SyllabusId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Version { get; init; } = null!;
    public string Title { get; init; } = null!;
    public bool IsActive { get; init; }
}
