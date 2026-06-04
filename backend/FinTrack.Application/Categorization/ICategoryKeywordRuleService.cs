using FinTrack.Application.Common;

namespace FinTrack.Application.Categorization;

public interface ICategoryKeywordRuleService
{
    Task<IReadOnlyList<CategoryKeywordRuleDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<CategoryKeywordRuleDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Result<CategoryKeywordRuleDto>> CreateAsync(
        Guid userId,
        CreateCategoryKeywordRuleRequest request,
        CancellationToken cancellationToken);
    Task<Result> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryKeywordRuleRequest request,
        CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
