namespace Kidzgo.API.Requests;

public sealed class UpdateProgramRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsMakeup { get; set; }
    public bool IsSupplementary { get; set; }
}
