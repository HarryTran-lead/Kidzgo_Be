using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlanTemplate : Entity
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Guid? SessionTemplateId { get; set; }
    public string? Title { get; set; }
    public int SessionIndex { get; set; }
    public int SessionOrder { get; set; }
    public string? SyllabusMetadata { get; set; }
    public string? SyllabusContent { get; set; }
    public string? Objectives { get; set; }
    public string? LanguageContent { get; set; }
    public string? Vocabulary { get; set; }
    public string? Grammar { get; set; }
    public string? TeachingMethodology { get; set; }
    public string? TeacherMaterials { get; set; }
    public string? StudentMaterials { get; set; }
    public string? Procedure { get; set; }
    public string? Evaluation { get; set; }
    public string? SourceFileName { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentMimeType { get; set; }  // MIME type: application/pdf, application/vnd.openxmlformats-officedocument.wordprocessingml.document, etc.
    public long? AttachmentFileSize { get; set; }  // Kích thước file (bytes)
    public string? AttachmentOriginalFileName { get; set; }  // Tên file gốc
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Module Module { get; set; } = null!;
    public SessionTemplate? SessionTemplate { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
    public ICollection<Sessions.Session> Sessions { get; set; } = new List<Sessions.Session>();
    public ICollection<LessonPlanTemplateActivity> Activities { get; set; } = new List<LessonPlanTemplateActivity>();
    public ICollection<LessonPlanTemplateMaterial> Materials { get; set; } = new List<LessonPlanTemplateMaterial>();
    public ICollection<HomeworkTemplate> HomeworkTemplates { get; set; } = new List<HomeworkTemplate>();
    public ICollection<Sessions.ClassSessionLesson> ClassSessionLessons { get; set; } = new List<Sessions.ClassSessionLesson>();
}
