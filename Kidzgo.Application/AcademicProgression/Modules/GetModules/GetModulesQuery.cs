using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.AcademicProgression.Modules.GetModules;

public sealed class GetModulesQuery : IQuery<GetModulesResponse>
{
    public Guid? LevelId { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}
