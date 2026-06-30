using FinTrack.Application.Categories;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Tests.Categories;

public class CategoryServiceTests
{
    [Fact]
    public async Task DeleteAsync_WhenCategoryHasTransactions_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Alimentacao",
            Type = CategoryType.Expense
        };
        var repository = new FakeCategoryRepository(
            new[] { category },
            categoriesWithTransactions: new[] { category.Id });
        var service = new CategoryService(repository);

        var result = await service.DeleteAsync(userId, category.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(BlockedDeletionMessage, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasChildren_ReturnsFriendlyFailure()
    {
        var userId = Guid.NewGuid();
        var category = CreateCategory(userId);
        var repository = new FakeCategoryRepository(
            new[] { category },
            categoriesWithChildren: new[] { category.Id });

        var result = await new CategoryService(repository)
            .DeleteAsync(userId, category.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Não é possível excluir esta categoria porque ela possui subcategorias.",
            result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasRecurringRules_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var category = CreateCategory(userId);
        var repository = new FakeCategoryRepository(
            new[] { category },
            categoriesWithRecurringRules: new[] { category.Id });

        var result = await new CategoryService(repository)
            .DeleteAsync(userId, category.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(BlockedDeletionMessage, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasKeywordRules_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var category = CreateCategory(userId);
        var repository = new FakeCategoryRepository(
            new[] { category },
            categoriesWithKeywordRules: new[] { category.Id });

        var result = await new CategoryService(repository)
            .DeleteAsync(userId, category.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(BlockedDeletionMessage, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryIsUnused_RemovesCategory()
    {
        var userId = Guid.NewGuid();
        var category = CreateCategory(userId);
        var repository = new FakeCategoryRepository(new[] { category });

        var result = await new CategoryService(repository)
            .DeleteAsync(userId, category.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(await repository.GetAllAsync(userId, CancellationToken.None));
    }

    private const string BlockedDeletionMessage =
        "Não é possível excluir esta categoria porque ela está sendo usada em transações, recorrências ou regras de categorização.";

    private static Category CreateCategory(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Alimentacao",
        Type = CategoryType.Expense
    };

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories;
        private readonly HashSet<Guid> _categoriesWithChildren;
        private readonly HashSet<Guid> _categoriesWithTransactions;
        private readonly HashSet<Guid> _categoriesWithRecurringRules;
        private readonly HashSet<Guid> _categoriesWithKeywordRules;

        public FakeCategoryRepository(
            IEnumerable<Category>? categories = null,
            IEnumerable<Guid>? categoriesWithChildren = null,
            IEnumerable<Guid>? categoriesWithTransactions = null,
            IEnumerable<Guid>? categoriesWithRecurringRules = null,
            IEnumerable<Guid>? categoriesWithKeywordRules = null)
        {
            _categories = categories?.ToList() ?? new List<Category>();
            _categoriesWithChildren = categoriesWithChildren?.ToHashSet() ?? new HashSet<Guid>();
            _categoriesWithTransactions = categoriesWithTransactions?.ToHashSet() ?? new HashSet<Guid>();
            _categoriesWithRecurringRules = categoriesWithRecurringRules?.ToHashSet() ?? new HashSet<Guid>();
            _categoriesWithKeywordRules = categoriesWithKeywordRules?.ToHashSet() ?? new HashSet<Guid>();
        }

        public Task AddAsync(Category category, CancellationToken cancellationToken)
        {
            _categories.Add(category);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categories.Any(category => category.UserId == userId && category.Id == id));
        }

        public Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Category>>(
                _categories.Where(category => category.UserId == userId).ToList());
        }

        public Task<Category?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                _categories.FirstOrDefault(category => category.UserId == userId && category.Id == id));
        }

        public Task<bool> HasChildrenAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categoriesWithChildren.Contains(id));
        }

        public Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categoriesWithTransactions.Contains(id));
        }

        public Task<bool> HasRecurringRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categoriesWithRecurringRules.Contains(id));
        }

        public Task<bool> HasKeywordRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categoriesWithKeywordRules.Contains(id));
        }

        public void Remove(Category category)
        {
            _categories.Remove(category);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
