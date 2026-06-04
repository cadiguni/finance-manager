using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Categorization;

public sealed class CategoryKeywordRuleService : ICategoryKeywordRuleService
{
    private readonly ICategoryKeywordRuleRepository _repository;
    private readonly ICategoryRepository _categoryRepository;

    public CategoryKeywordRuleService(
        ICategoryKeywordRuleRepository repository,
        ICategoryRepository categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryKeywordRuleDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rules = await _repository.GetAllAsync(userId, cancellationToken);
        return rules.Select(MapToDto).ToList();
    }

    public async Task<CategoryKeywordRuleDto?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(userId, id, cancellationToken);
        return rule is null ? null : MapToDto(rule);
    }

    public async Task<Result<CategoryKeywordRuleDto>> CreateAsync(
        Guid userId,
        CreateCategoryKeywordRuleRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateAsync(
            userId,
            request.CategoryId,
            request.Keyword,
            request.TransactionType,
            null,
            cancellationToken);
        if (validationError is not null)
        {
            return Result<CategoryKeywordRuleDto>.Failure(validationError);
        }

        var rule = new CategoryKeywordRule
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Keyword = request.Keyword.Trim(),
            TransactionType = request.TransactionType,
            Priority = Math.Max(0, request.Priority),
            IsActive = request.IsActive
        };

        await _repository.AddAsync(rule, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        rule.Category = await _categoryRepository.GetByIdAsync(userId, rule.CategoryId, cancellationToken);

        return Result<CategoryKeywordRuleDto>.Success(MapToDto(rule));
    }

    public async Task<Result> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryKeywordRuleRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(userId, id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure("Keyword rule not found.");
        }

        var validationError = await ValidateAsync(
            userId,
            request.CategoryId,
            request.Keyword,
            request.TransactionType,
            id,
            cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        rule.CategoryId = request.CategoryId;
        rule.Keyword = request.Keyword.Trim();
        rule.TransactionType = request.TransactionType;
        rule.Priority = Math.Max(0, request.Priority);
        rule.IsActive = request.IsActive;

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(userId, id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure("Keyword rule not found.");
        }

        _repository.Remove(rule);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<string?> ValidateAsync(
        Guid userId,
        Guid categoryId,
        string keyword,
        Domain.Enums.TransactionType? transactionType,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return "Keyword is required.";
        }

        if (keyword.Trim().Length < 2)
        {
            return "Keyword must have at least 2 characters.";
        }

        var category = await _categoryRepository.GetByIdAsync(userId, categoryId, cancellationToken);
        if (category is null)
        {
            return "Category not found.";
        }

        if (category.Type != Domain.Enums.CategoryType.Both)
        {
            if (transactionType is null)
            {
                return "Transaction type is required when category is not Both.";
            }

            if ((transactionType == Domain.Enums.TransactionType.Income &&
                 category.Type != Domain.Enums.CategoryType.Income) ||
                (transactionType == Domain.Enums.TransactionType.Expense &&
                 category.Type != Domain.Enums.CategoryType.Expense))
            {
                return "Category type is incompatible with transaction type.";
            }
        }

        if (await _repository.ExistsAsync(userId, keyword.Trim(), transactionType, exceptId, cancellationToken))
        {
            return "Keyword rule already exists.";
        }

        return null;
    }

    private static CategoryKeywordRuleDto MapToDto(CategoryKeywordRule rule)
    {
        return new CategoryKeywordRuleDto(
            rule.Id,
            rule.CategoryId,
            rule.Category?.Name ?? string.Empty,
            rule.Keyword,
            rule.TransactionType,
            rule.Priority,
            rule.IsActive,
            rule.CreatedAt);
    }
}
