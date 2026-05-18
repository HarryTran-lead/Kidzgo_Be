using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlanTemplate : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public Guid? ModuleId { get; set; }
    public string? Level { get; set; }
    public string? Title { get; set; }
    public int SessionIndex { get; set; }
    public int SessionOrder { get; set; }
    public string? SyllabusMetadata { get; set; }
    public string? SyllabusContent { get; set; }
    public string? SourceFileName { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentMimeType { get; set; }  // MIME type: application/pdf, application/vnd.openxmlformats-officedocument.wordprocessingml.document, etc.
    public long? AttachmentFileSize { get; set; }  // Kích thước file (bytes)
    public string? AttachmentOriginalFileName { get; set; }  // Tên file gốc
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Program Program { get; set; } = null!;
    public Module? Module { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
}
