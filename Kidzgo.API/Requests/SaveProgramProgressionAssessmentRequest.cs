using System.Text.Json.Serialization;
using Kidzgo.API.Extensions;

namespace Kidzgo.API.Requests;

public sealed class SaveProgramProgressionAssessmentRequest
{
    public Guid? SourceRegistrationId { get; set; }
    public Guid? ScheduleParticipantId { get; set; }
    public DateTime? AssessmentDate { get; set; }
    public bool? PassedInClass { get; set; }
    public decimal? ListeningScore { get; set; }
    public decimal? SpeakingScore { get; set; }
    public decimal? ReadingWritingScore { get; set; }
    public decimal? ReadingScore { get; set; }
    public decimal? WritingScore { get; set; }
    public string? Comment { get; set; }

    [JsonConverter(typeof(StringOrStringArrayJsonConverter))]
    public List<string>? AttachmentUrls { get; set; }
}
