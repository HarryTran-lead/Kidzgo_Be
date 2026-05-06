using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionAssessment;

public sealed class CreateProgramProgressionAssessmentCommand : ICommand<ProgramProgressionAssessmentDto>
{
    public Guid? SourceRegistrationId { get; init; }
    public Guid? ScheduleParticipantId { get; init; }
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
