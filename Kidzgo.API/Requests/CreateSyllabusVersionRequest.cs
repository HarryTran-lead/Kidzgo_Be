namespace Kidzgo.API.Requests;

public sealed class CreateSyllabusVersionRequest
{
    public int Version { get; init; }
    public string? Title { get; init; }
    public string? Edition { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool PromoteNow { get; init; }
}
