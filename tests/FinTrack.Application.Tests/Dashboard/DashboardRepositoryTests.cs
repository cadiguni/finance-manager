using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Dashboard;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Tests.Dashboard;

public class DashboardRepositoryTests
{
    [Fact]
    public async Task GetMonthlySummaryAsync_WithInitialBalanceAndNoTransactions_ReturnsInitialCurrentBalance()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Accounts.Add(CreateAccount(userId, 1_000m));
        await context.SaveChangesAsync();

        var summary = await new DashboardRepository(context)
            .GetMonthlySummaryAsync(userId, 2026, 6, CancellationToken.None);

        Assert.Equal(0m, summary.Balance);
        Assert.Equal(1_000m, summary.InitialBalance);
        Assert.Equal(1_000m, summary.CurrentBalance);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WithIncomeAndExpense_SeparatesMonthlyAndAccumulatedBalances()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateContext();
        var account = CreateAccount(userId, 1_000m);
        var incomeCategory = CreateCategory(userId, CategoryType.Income, "Salário");
        var expenseCategory = CreateCategory(userId, CategoryType.Expense, "Alimentação");
        context.AddRange(account, incomeCategory, expenseCategory);
        context.Transactions.AddRange(
            CreateTransaction(userId, account.Id, expenseCategory.Id, 100m, TransactionType.Expense, new DateOnly(2026, 5, 20)),
            CreateTransaction(userId, account.Id, incomeCategory.Id, 500m, TransactionType.Income, new DateOnly(2026, 6, 5)),
            CreateTransaction(userId, account.Id, expenseCategory.Id, 200m, TransactionType.Expense, new DateOnly(2026, 6, 10)),
            CreateTransaction(userId, account.Id, expenseCategory.Id, 900m, TransactionType.Expense, new DateOnly(2026, 7, 1)));
        await context.SaveChangesAsync();

        var summary = await new DashboardRepository(context)
            .GetMonthlySummaryAsync(userId, 2026, 6, CancellationToken.None);

        Assert.Equal(500m, summary.TotalIncome);
        Assert.Equal(200m, summary.TotalExpense);
        Assert.Equal(300m, summary.Balance);
        Assert.Equal(1_000m, summary.InitialBalance);
        Assert.Equal(1_200m, summary.CurrentBalance);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WithMultipleAccounts_SumsTheirInitialBalances()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Accounts.AddRange(
            CreateAccount(userId, 1_000m),
            CreateAccount(userId, 250m),
            CreateAccount(Guid.NewGuid(), 9_999m));
        await context.SaveChangesAsync();

        var summary = await new DashboardRepository(context)
            .GetMonthlySummaryAsync(userId, 2026, 6, CancellationToken.None);

        Assert.Equal(1_250m, summary.InitialBalance);
        Assert.Equal(1_250m, summary.CurrentBalance);
    }

    private static FinTrackDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FinTrackDbContext(options);
    }

    private static Account CreateAccount(Guid userId, decimal initialBalance) => new()
    {
        UserId = userId,
        Name = $"Conta {Guid.NewGuid():N}",
        Type = AccountType.BankAccount,
        InitialBalance = initialBalance
    };

    private static Category CreateCategory(Guid userId, CategoryType type, string name) => new()
    {
        UserId = userId,
        Name = name,
        Type = type
    };

    private static Transaction CreateTransaction(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        decimal amount,
        TransactionType type,
        DateOnly date) => new()
    {
        UserId = userId,
        AccountId = accountId,
        CategoryId = categoryId,
        Description = "Teste",
        Amount = amount,
        Type = type,
        Date = date
    };
}
