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
        Assert.Equal("Category has transactions and cannot be deleted.", result.Error);
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories;
        private readonly HashSet<Guid> _categoriesWithTransactions;

        public FakeCategoryRepository(
            IEnumerable<Category>? categories = null,
            IEnumerable<Guid>? categoriesWithTransactions = null)
        {
            _categories = categories?.ToList() ?? new List<Category>();
            _categoriesWithTransactions = categoriesWithTransactions?.ToHashSet() ?? new HashSet<Guid>();
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
            return Task.FromResult(false);
        }

        public Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categoriesWithTransactions.Contains(id));
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
