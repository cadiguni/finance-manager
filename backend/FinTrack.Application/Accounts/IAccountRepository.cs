using FinTrack.Domain.Entities;

namespace FinTrack.Application.Accounts;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<Account?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> HasRecurringRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(Account account, CancellationToken cancellationToken);
    void Remove(Account account);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
