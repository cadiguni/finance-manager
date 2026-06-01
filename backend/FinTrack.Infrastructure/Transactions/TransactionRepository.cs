using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Transactions;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly FinTrackDbContext _dbContext;

    public TransactionRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(
        Guid userId,
        TransactionFilters filters,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId);

        if (filters.StartDate.HasValue)
        {
            query = query.Where(transaction => transaction.Date >= filters.StartDate.Value);
        }

        if (filters.EndDate.HasValue)
        {
            query = query.Where(transaction => transaction.Date <= filters.EndDate.Value);
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(transaction => transaction.CategoryId == filters.CategoryId.Value);
        }

        if (filters.AccountId.HasValue)
        {
            query = query.Where(transaction => transaction.AccountId == filters.AccountId.Value);
        }

        if (filters.Type.HasValue)
        {
            query = query.Where(transaction => transaction.Type == filters.Type.Value);
        }

        if (filters.IsPaid.HasValue)
        {
            query = query.Where(transaction => transaction.IsPaid == filters.IsPaid.Value);
        }

        return await query
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .FirstOrDefaultAsync(transaction => transaction.UserId == userId && transaction.Id == id, cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }

    public void Remove(Transaction transaction)
    {
        _dbContext.Transactions.Remove(transaction);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
