using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class CategoryKeywordRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public TransactionType? TransactionType { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Category? Category { get; set; }
}
