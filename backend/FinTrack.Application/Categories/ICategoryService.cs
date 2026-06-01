using FinTrack.Application.Common;

namespace FinTrack.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<CategoryDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
