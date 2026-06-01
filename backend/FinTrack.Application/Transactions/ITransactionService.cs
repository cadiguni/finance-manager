using FinTrack.Application.Common;

namespace FinTrack.Application.Transactions;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionDto>> GetAllAsync(
        Guid userId,
        TransactionFilters filters,
        CancellationToken cancellationToken);

    Task<TransactionDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Result<TransactionDto>> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(Guid userId, Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
