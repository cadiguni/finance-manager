using FinTrack.Application.Accounts;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Tests.Accounts;

public class AccountServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ReturnsFailure()
    {
        var service = new AccountService(new FakeAccountRepository());

        var result = await service.CreateAsync(
            Guid.NewGuid(),
            new CreateAccountRequest("", AccountType.BankAccount, 100m),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account name is required.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenAccountHasTransactions_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Conta Principal",
            Type = AccountType.BankAccount
        };
        var repository = new FakeAccountRepository(new[] { account }, accountsWithTransactions: new[] { account.Id });
        var service = new AccountService(repository);

        var result = await service.DeleteAsync(userId, account.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Não é possível excluir esta conta porque ela está sendo usada em transações ou recorrências.",
            result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenAccountHasRecurringRules_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var account = CreateAccount(userId);
        var repository = new FakeAccountRepository(
            new[] { account },
            accountsWithRecurringRules: new[] { account.Id });

        var result = await new AccountService(repository)
            .DeleteAsync(userId, account.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Não é possível excluir esta conta porque ela está sendo usada em transações ou recorrências.",
            result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenAccountIsUnused_RemovesAccount()
    {
        var userId = Guid.NewGuid();
        var account = CreateAccount(userId);
        var repository = new FakeAccountRepository(new[] { account });

        var result = await new AccountService(repository)
            .DeleteAsync(userId, account.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(await repository.GetAllAsync(userId, CancellationToken.None));
    }

    private static Account CreateAccount(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Conta Principal",
        Type = AccountType.BankAccount
    };

    [Fact]
    public async Task UpdateAsync_WhenAccountExists_UpdatesAccount()
    {
        var userId = Guid.NewGuid();
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Conta Antiga",
            Type = AccountType.Cash,
            InitialBalance = 10m
        };
        var repository = new FakeAccountRepository(new[] { account });
        var service = new AccountService(repository);

        var result = await service.UpdateAsync(
            userId,
            account.Id,
            new UpdateAccountRequest("Conta Nova", AccountType.Investment, 500m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Conta Nova", account.Name);
        Assert.Equal(AccountType.Investment, account.Type);
        Assert.Equal(500m, account.InitialBalance);
    }

    private sealed class FakeAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts;
        private readonly HashSet<Guid> _accountsWithTransactions;
        private readonly HashSet<Guid> _accountsWithRecurringRules;

        public FakeAccountRepository(
            IEnumerable<Account>? accounts = null,
            IEnumerable<Guid>? accountsWithTransactions = null,
            IEnumerable<Guid>? accountsWithRecurringRules = null)
        {
            _accounts = accounts?.ToList() ?? new List<Account>();
            _accountsWithTransactions = accountsWithTransactions?.ToHashSet() ?? new HashSet<Guid>();
            _accountsWithRecurringRules = accountsWithRecurringRules?.ToHashSet() ?? new HashSet<Guid>();
        }

        public Task AddAsync(Account account, CancellationToken cancellationToken)
        {
            _accounts.Add(account);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Account>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Account>>(
                _accounts.Where(account => account.UserId == userId).ToList());
        }

        public Task<Account?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                _accounts.FirstOrDefault(account => account.UserId == userId && account.Id == id));
        }

        public Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_accountsWithTransactions.Contains(id));
        }

        public Task<bool> HasRecurringRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_accountsWithRecurringRules.Contains(id));
        }

        public void Remove(Account account)
        {
            _accounts.Remove(account);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
