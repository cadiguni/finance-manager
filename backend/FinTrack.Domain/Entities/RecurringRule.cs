using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class RecurringRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public RecurringFrequency Frequency { get; set; }
    public int DayOfMonth { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Guid AccountId { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
    public Account? Account { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
