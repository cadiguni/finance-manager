using FinTrack.Domain.Entities;

namespace FinTrack.Application.Categories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> HasChildrenAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    void Remove(Category category);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
