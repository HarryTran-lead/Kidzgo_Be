using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Faqs;

public class FaqItem : Entity
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public FaqCategory Category { get; set; } = null!;
}
