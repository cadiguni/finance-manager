using FinTrack.Application.Recurring;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Recurring;

public sealed class RecurringRuleRepository : IRecurringRuleRepository
{
    private readonly FinTrackDbContext _dbContext;

    public RecurringRuleRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecurringRule>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RecurringRules
            .AsNoTracking()
            .Where(rule => rule.UserId == userId)
            .OrderBy(rule => rule.Description)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringRule>> GetActiveAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RecurringRules
            .AsNoTracking()
            .Where(rule => rule.UserId == userId && rule.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecurringRule?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RecurringRules
            .FirstOrDefaultAsync(rule => rule.UserId == userId && rule.Id == id, cancellationToken);
    }

    public async Task AddAsync(RecurringRule rule, CancellationToken cancellationToken)
    {
        await _dbContext.RecurringRules.AddAsync(rule, cancellationToken);
    }

    public async Task<bool> HasTransactionAsync(
        Guid userId,
        Guid ruleId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions.AnyAsync(
            transaction =>
                transaction.UserId == userId &&
                transaction.RecurringRuleId == ruleId &&
                transaction.Date == date,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
