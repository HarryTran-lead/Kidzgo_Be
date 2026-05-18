using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.Modules.GetModules;

public sealed class GetModulesResponse
{
    public List<ModuleDto> Items { get; init; } = [];
}
