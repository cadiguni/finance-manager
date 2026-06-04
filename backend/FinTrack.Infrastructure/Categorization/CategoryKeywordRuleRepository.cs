using FinTrack.Application.Categorization;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Categorization;

public sealed class CategoryKeywordRuleRepository : ICategoryKeywordRuleRepository
{
    private readonly FinTrackDbContext _dbContext;

    public CategoryKeywordRuleRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryKeywordRule>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CategoryKeywordRules
            .AsNoTracking()
            .Include(rule => rule.Category)
            .Where(rule => rule.UserId == userId)
            .OrderByDescending(rule => rule.Priority)
            .ThenBy(rule => rule.Keyword)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryKeywordRule?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CategoryKeywordRules
            .Include(rule => rule.Category)
            .FirstOrDefaultAsync(rule => rule.UserId == userId && rule.Id == id, cancellationToken);
    }

    public async Task<CategoryKeywordRule?> FindMatchAsync(
        Guid userId,
        string description,
        TransactionType transactionType,
        CancellationToken cancellationToken)
    {
        var normalizedDescription = description.ToLowerInvariant();
        var candidates = await _dbContext.CategoryKeywordRules
            .AsNoTracking()
            .Include(rule => rule.Category)
            .Where(rule =>
                rule.UserId == userId &&
                rule.IsActive &&
                (rule.TransactionType == null || rule.TransactionType == transactionType))
            .OrderByDescending(rule => rule.Priority)
            .ThenByDescending(rule => rule.Keyword.Length)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(rule =>
            normalizedDescription.Contains(rule.Keyword.ToLowerInvariant(), StringComparison.Ordinal));
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        string keyword,
        TransactionType? transactionType,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        return await _dbContext.CategoryKeywordRules
            .AnyAsync(rule =>
                rule.UserId == userId &&
                rule.Id != exceptId &&
                rule.Keyword.ToLower() == normalizedKeyword &&
                rule.TransactionType == transactionType,
                cancellationToken);
    }

    public async Task AddAsync(CategoryKeywordRule rule, CancellationToken cancellationToken)
    {
        await _dbContext.CategoryKeywordRules.AddAsync(rule, cancellationToken);
    }

    public void Remove(CategoryKeywordRule rule)
    {
        _dbContext.CategoryKeywordRules.Remove(rule);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
