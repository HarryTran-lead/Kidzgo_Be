using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Schools;
using DomainProgram = Kidzgo.Domain.Programs.Program;
using DomainLevel = Kidzgo.Domain.Programs.Level;

namespace Kidzgo.Domain.CRM;

public class PlacementTest : Entity
{
    public Guid Id { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? LeadChildId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public PlacementTestStatus Status { get; set; }
    public Guid? RoomId { get; set; }
    public string? Room { get; set; }
    public Guid? InvigilatorUserId { get; set; }
    public Guid? OriginalPlacementTestId { get; set; }
    public decimal? ResultScore { get; set; }
    public decimal? ListeningScore { get; set; }
    public decimal? SpeakingScore { get; set; }
    public decimal? ReadingScore { get; set; }
    public decimal? WritingScore { get; set; }
    public string? LevelRecommendation { get; set; }
    public Guid? ProgramRecommendationId { get; set; }
    public Guid? PrimaryLevelRecommendationId { get; set; }
    public Guid? SecondaryLevelRecommendationId { get; set; }
    public Guid? SecondaryProgramRecommendationId { get; set; }
    public string? SecondaryProgramSkillFocus { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Lead? Lead { get; set; }
    public LeadChild? LeadChild { get; set; }
    public Profile? StudentProfile { get; set; }
    public Class? Class { get; set; }
    public Classroom? PlacementRoom { get; set; }
    public User? InvigilatorUser { get; set; }
    public PlacementTest? OriginalPlacementTest { get; set; }
    public DomainProgram? ProgramRecommendationProgram { get; set; }
    public DomainLevel? PrimaryLevelRecommendationLevel { get; set; }
    public DomainLevel? SecondaryLevelRecommendationLevel { get; set; }
    public DomainProgram? SecondaryProgramRecommendationProgram { get; set; }
}
