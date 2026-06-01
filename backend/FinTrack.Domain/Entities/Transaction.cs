using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly? DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public Guid? InstallmentGroupId { get; set; }
    public Guid? RecurringRuleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public Category? Category { get; set; }
    public InstallmentGroup? InstallmentGroup { get; set; }
    public RecurringRule? RecurringRule { get; set; }
}
