using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlan : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SessionId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? PlannedContent { get; set; }
    public string? ActualContent { get; set; }
    public string? ActualHomework { get; set; }
    public string? TeacherNotes { get; set; }
    public decimal? CompletionPercent { get; set; }
    public string? CarryForwardContent { get; set; }
    
    // Media/Attachment fields
    public string? CoverImageUrl { get; set; }  // Ảnh cover của giáo án
    public string? CoverImageMimeType { get; set; }
    public long? CoverImageFileSize { get; set; }
    public string? MediaUrl { get; set; }  // Video/media đính kèm
    public string? MediaMimeType { get; set; }
    public long? MediaFileSize { get; set; }
    public string? MediaType { get; set; }  // "image" hoặc "video"
    
    public Guid? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Session Session { get; set; } = null!;
    public LessonPlanTemplate? Template { get; set; }
    public User? SubmittedByUser { get; set; }
    public Sessions.TeachingLog? TeachingLog { get; set; }
}
