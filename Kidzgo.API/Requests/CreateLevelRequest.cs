namespace Kidzgo.API.Requests;

public sealed class CreateLevelRequest
{
    public Guid ProgramId { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Order { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
