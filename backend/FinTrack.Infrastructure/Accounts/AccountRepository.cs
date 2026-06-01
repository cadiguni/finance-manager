using FinTrack.Application.Accounts;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Accounts;

public sealed class AccountRepository : IAccountRepository
{
    private readonly FinTrackDbContext _dbContext;

    public AccountRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Account>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .OrderBy(account => account.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(account => account.UserId == userId && account.Id == id, cancellationToken);
    }

    public async Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AnyAsync(transaction => transaction.UserId == userId && transaction.AccountId == id, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public void Remove(Account account)
    {
        _dbContext.Accounts.Remove(account);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
