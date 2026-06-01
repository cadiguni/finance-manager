using FinTrack.Domain.Entities;

namespace FinTrack.Application.Transactions;

public interface ITransactionRepository
{
    Task<IReadOnlyList<Transaction>> GetAllAsync(
        Guid userId,
        TransactionFilters filters,
        CancellationToken cancellationToken);

    Task<Transaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
    void Remove(Transaction transaction);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
