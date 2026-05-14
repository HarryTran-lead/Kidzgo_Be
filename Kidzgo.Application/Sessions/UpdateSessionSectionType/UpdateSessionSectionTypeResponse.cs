namespace Kidzgo.Application.Sessions.UpdateSessionSectionType;

public sealed class UpdateSessionSectionTypeResponse
{
    public Guid Id { get; init; }
    public string SectionType { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
