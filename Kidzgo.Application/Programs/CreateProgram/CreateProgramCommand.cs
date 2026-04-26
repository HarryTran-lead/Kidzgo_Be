using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramCommand : ICommand<CreateProgramResponse>
{
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsMakeup { get; init; }
    public bool IsSupplementary { get; init; }
}
