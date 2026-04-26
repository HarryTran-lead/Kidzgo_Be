namespace Kidzgo.API.Requests;

public sealed class CreateProgramRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsMakeup { get; set; }
    public bool IsSupplementary { get; set; }
}
