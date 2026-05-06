using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionAssessment;

public sealed class UpdateProgramProgressionAssessmentCommand : ICommand<ProgramProgressionAssessmentDto>
{
    public Guid Id { get; init; }
    public DateTime? AssessmentDate { get; init; }
    public bool? PassedInClass { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingWritingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public string? Comment { get; init; }
    public IReadOnlyCollection<string>? AttachmentUrls { get; init; }
}
