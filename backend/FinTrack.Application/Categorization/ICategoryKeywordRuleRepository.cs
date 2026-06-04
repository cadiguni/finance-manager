using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categorization;

public interface ICategoryKeywordRuleRepository
{
    Task<IReadOnlyList<CategoryKeywordRule>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<CategoryKeywordRule?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<CategoryKeywordRule?> FindMatchAsync(
        Guid userId,
        string description,
        TransactionType transactionType,
        CancellationToken cancellationToken);
    Task<bool> ExistsAsync(
        Guid userId,
        string keyword,
        TransactionType? transactionType,
        Guid? exceptId,
        CancellationToken cancellationToken);
    Task AddAsync(CategoryKeywordRule rule, CancellationToken cancellationToken);
    void Remove(CategoryKeywordRule rule);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
