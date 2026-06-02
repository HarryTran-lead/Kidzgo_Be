namespace Kidzgo.API.Requests;

public sealed class CreateSyllabusVersionRequest
{
    public string Version { get; init; } = null!;
    public string? Title { get; init; }
    public string? Edition { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool PromoteNow { get; init; }
}
