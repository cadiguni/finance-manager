using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public sealed record UpdateTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    string Description,
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    DateOnly? DueDate,
    bool IsPaid,
    DateOnly? PaymentDate);
