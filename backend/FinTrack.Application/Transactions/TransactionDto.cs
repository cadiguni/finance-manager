using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    string Description,
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    DateOnly? DueDate,
    bool IsPaid,
    DateOnly? PaymentDate,
    Guid? InstallmentGroupId,
    Guid? RecurringRuleId,
    DateTime CreatedAt);
