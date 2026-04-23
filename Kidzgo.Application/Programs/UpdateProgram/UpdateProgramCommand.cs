using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.UpdateProgram;

public sealed class UpdateProgramCommand : ICommand<UpdateProgramResponse>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
    public bool IsMakeup { get; init; }
    public bool IsSupplementary { get; init; }
}
