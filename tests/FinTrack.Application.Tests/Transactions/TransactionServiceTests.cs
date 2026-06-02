using FinTrack.Application.Accounts;
using FinTrack.Application.Categories;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Tests.Transactions;

public class TransactionServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenPaidWithoutPaymentDate_ReturnsFailure()
    {
        var fixture = TestFixture.Create();

        var result = await fixture.Service.CreateAsync(
            fixture.UserId,
            fixture.ValidRequest with { IsPaid = true, PaymentDate = null },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Payment date is required when the transaction is paid.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryTypeIsIncompatible_ReturnsFailure()
    {
        var fixture = TestFixture.Create();

        var result = await fixture.Service.CreateAsync(
            fixture.UserId,
            fixture.ValidRequest with { Type = TransactionType.Income },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category type is incompatible with transaction type.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_AddsTransaction()
    {
        var fixture = TestFixture.Create();

        var result = await fixture.Service.CreateAsync(
            fixture.UserId,
            fixture.ValidRequest,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.TransactionRepository.Transactions);
        Assert.Equal("Mercado", fixture.TransactionRepository.Transactions[0].Description);
    }

    private sealed record TestFixture(
        Guid UserId,
        TransactionService Service,
        FakeTransactionRepository TransactionRepository,
        CreateTransactionRequest ValidRequest)
    {
        public static TestFixture Create()
        {
            var userId = Guid.NewGuid();
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Conta",
                Type = AccountType.BankAccount
            };
            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Alimentacao",
                Type = CategoryType.Expense
            };

            var transactionRepository = new FakeTransactionRepository();
            var service = new TransactionService(
                transactionRepository,
                new FakeAccountRepository(new[] { account }),
                new FakeCategoryRepository(new[] { category }));

            var request = new CreateTransactionRequest(
                account.Id,
                category.Id,
                "Mercado",
                120m,
                TransactionType.Expense,
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 5),
                false,
                null);

            return new TestFixture(userId, service, transactionRepository, request);
        }
    }

    private sealed class FakeTransactionRepository : ITransactionRepository
    {
        public List<Transaction> Transactions { get; } = new();

        public Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Transaction>> GetAllAsync(
            Guid userId,
            TransactionFilters filters,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Transaction>>(
                Transactions.Where(transaction => transaction.UserId == userId).ToList());
        }

        public Task<Transaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Transactions.FirstOrDefault(transaction => transaction.UserId == userId && transaction.Id == id));
        }

        public void Remove(Transaction transaction)
        {
            Transactions.Remove(transaction);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts;

        public FakeAccountRepository(IEnumerable<Account> accounts)
        {
            _accounts = accounts.ToList();
        }

        public Task AddAsync(Account account, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<Account>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Account>>(_accounts);
        }

        public Task<Account?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.UserId == userId && account.Id == id));
        }

        public Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public void Remove(Account account)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories;

        public FakeCategoryRepository(IEnumerable<Category> categories)
        {
            _categories = categories.ToList();
        }

        public Task AddAsync(Category category, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categories.Any(category => category.UserId == userId && category.Id == id));
        }

        public Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Category>>(_categories);
        }

        public Task<Category?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categories.FirstOrDefault(category => category.UserId == userId && category.Id == id));
        }

        public Task<bool> HasChildrenAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public void Remove(Category category)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
