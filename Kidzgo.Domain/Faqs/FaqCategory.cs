using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Faqs;

public class FaqCategory : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<FaqItem> FaqItems { get; set; } = new List<FaqItem>();
}
