using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync(userId, cancellationToken);

        return categories
            .Select(MapToDto)
            .ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(userId, id, cancellationToken);

        return category is null ? null : MapToDto(category);
    }

    public async Task<Result<CategoryDto>> CreateAsync(
        Guid userId,
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateAsync(userId, request.Name, request.ParentCategoryId, cancellationToken);
        if (validationError is not null)
        {
            return Result<CategoryDto>.Failure(validationError);
        }

        var category = new Category
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type,
            ParentCategoryId = request.ParentCategoryId
        };

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(userId, id, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Category not found.");
        }

        if (request.ParentCategoryId == id)
        {
            return Result.Failure("A category cannot be its own parent.");
        }

        var validationError = await ValidateAsync(userId, request.Name, request.ParentCategoryId, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        category.Name = request.Name.Trim();
        category.Type = request.Type;
        category.ParentCategoryId = request.ParentCategoryId;

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(userId, id, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Category not found.");
        }

        if (await _repository.HasChildrenAsync(userId, id, cancellationToken))
        {
            return Result.Failure("Category has subcategories and cannot be deleted.");
        }

        if (await _repository.HasTransactionsAsync(userId, id, cancellationToken) ||
            await _repository.HasRecurringRulesAsync(userId, id, cancellationToken) ||
            await _repository.HasKeywordRulesAsync(userId, id, cancellationToken))
        {
            return Result.Failure(
                "Não é possível excluir esta categoria porque ela está sendo usada em transações, recorrências ou regras de categorização.");
        }

        _repository.Remove(category);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<string?> ValidateAsync(
        Guid userId,
        string name,
        Guid? parentCategoryId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Category name is required.";
        }

        if (parentCategoryId.HasValue &&
            !await _repository.ExistsAsync(userId, parentCategoryId.Value, cancellationToken))
        {
            return "Parent category not found.";
        }

        return null;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Type,
            category.ParentCategoryId,
            category.CreatedAt);
    }
}
