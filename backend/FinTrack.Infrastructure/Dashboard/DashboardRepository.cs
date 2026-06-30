using FinTrack.Application.Dashboard;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Dashboard;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly FinTrackDbContext _dbContext;

    public DashboardRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date >= startDate &&
                transaction.Date <= endDate);

        var totalIncome = await transactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        var totalExpense = await transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        var initialBalance = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .SumAsync(account => account.InitialBalance, cancellationToken);

        var accumulatedIncome = await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date <= endDate &&
                transaction.Type == TransactionType.Income)
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        var accumulatedExpense = await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date <= endDate &&
                transaction.Type == TransactionType.Expense)
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        var expensesByCategoryRows = await transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category!.Name
            })
            .Select(group => new
            {
                group.Key.CategoryId,
                group.Key.CategoryName,
                Total = group.Sum(transaction => transaction.Amount)
            })
            .OrderByDescending(summary => summary.Total)
            .ToListAsync(cancellationToken);

        var incomeByCategoryRows = await transactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category!.Name
            })
            .Select(group => new
            {
                group.Key.CategoryId,
                group.Key.CategoryName,
                Total = group.Sum(transaction => transaction.Amount)
            })
            .OrderByDescending(summary => summary.Total)
            .ToListAsync(cancellationToken);

        var paidExpenses = await transactions
            .Where(transaction => transaction.Type == TransactionType.Expense && transaction.IsPaid)
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        var unpaidExpenses = await transactions
            .Where(transaction => transaction.Type == TransactionType.Expense && !transaction.IsPaid)
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        var upcomingPayments = await transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense &&
                !transaction.IsPaid &&
                transaction.DueDate.HasValue &&
                transaction.DueDate.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .SumAsync(transaction => transaction.Amount, cancellationToken);

        return new MonthlySummaryDto(
            year,
            month,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            initialBalance,
            initialBalance + accumulatedIncome - accumulatedExpense,
            expensesByCategoryRows
                .Select(summary => new CategorySummaryDto(summary.CategoryId, summary.CategoryName, summary.Total))
                .ToList(),
            incomeByCategoryRows
                .Select(summary => new CategorySummaryDto(summary.CategoryId, summary.CategoryName, summary.Total))
                .ToList(),
            upcomingPayments,
            paidExpenses,
            unpaidExpenses);
    }
}
