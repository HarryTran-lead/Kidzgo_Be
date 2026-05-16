using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.Modules.UpdateModule;

public sealed class UpdateModuleCommand : ICommand<ModuleDto>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Order { get; init; }
    public string? Description { get; init; }
    public int PlannedSessionCount { get; init; }
    public bool IsActive { get; init; }
}
