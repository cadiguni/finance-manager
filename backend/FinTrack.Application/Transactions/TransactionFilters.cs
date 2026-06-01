using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public sealed record TransactionFilters(
    DateOnly? StartDate,
    DateOnly? EndDate,
    Guid? CategoryId,
    Guid? AccountId,
    TransactionType? Type,
    bool? IsPaid);
