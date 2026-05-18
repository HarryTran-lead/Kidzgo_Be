namespace Kidzgo.API.Requests;

public sealed class UpdateModuleRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Order { get; init; }
    public string? Description { get; init; }
    public int PlannedSessionCount { get; init; }
    public bool IsActive { get; init; } = true;
}
