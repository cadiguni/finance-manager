using FinTrack.Application.Categories;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Categories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly FinTrackDbContext _dbContext;

    public CategoryRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(category => category.UserId == userId && category.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AnyAsync(category => category.UserId == userId && category.Id == id, cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AnyAsync(category => category.UserId == userId && category.ParentCategoryId == id, cancellationToken);
    }

    public async Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AnyAsync(transaction => transaction.UserId == userId && transaction.CategoryId == id, cancellationToken);
    }

    public async Task<bool> HasRecurringRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.RecurringRules
            .AnyAsync(rule => rule.UserId == userId && rule.CategoryId == id, cancellationToken);
    }

    public async Task<bool> HasKeywordRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.CategoryKeywordRules
            .AnyAsync(rule => rule.UserId == userId && rule.CategoryId == id, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
    }

    public void Remove(Category category)
    {
        _dbContext.Categories.Remove(category);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
