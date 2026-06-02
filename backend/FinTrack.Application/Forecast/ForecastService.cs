using FinTrack.Application.Recurring;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Forecast;

public sealed class ForecastService : IForecastService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IRecurringRuleRepository _recurringRuleRepository;

    public ForecastService(
        ITransactionRepository transactionRepository,
        IRecurringRuleRepository recurringRuleRepository)
    {
        _transactionRepository = transactionRepository;
        _recurringRuleRepository = recurringRuleRepository;
    }

    public async Task<IReadOnlyList<ForecastMonthDto>> GetNextMonthsAsync(
        Guid userId,
        int startYear,
        int startMonth,
        int months,
        CancellationToken cancellationToken)
    {
        months = Math.Clamp(months, 1, 24);

        var startDate = new DateOnly(startYear, startMonth, 1);
        var endMonth = startDate.AddMonths(months - 1);
        var endDate = new DateOnly(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));

        var transactions = await _transactionRepository.GetAllAsync(
            userId,
            new TransactionFilters(startDate, endDate, null, null, null, null),
            cancellationToken);
        var recurringRules = await _recurringRuleRepository.GetActiveAsync(userId, cancellationToken);

        var result = new List<ForecastMonthDto>();
        for (var index = 0; index < months; index++)
        {
            var monthDate = startDate.AddMonths(index);
            var monthStart = new DateOnly(monthDate.Year, monthDate.Month, 1);
            var monthEnd = new DateOnly(monthDate.Year, monthDate.Month, DateTime.DaysInMonth(monthDate.Year, monthDate.Month));

            var monthTransactions = transactions
                .Where(transaction => transaction.Date >= monthStart && transaction.Date <= monthEnd)
                .ToList();

            var income = monthTransactions
                .Where(transaction => transaction.Type == TransactionType.Income)
                .Sum(transaction => transaction.Amount);
            var expense = monthTransactions
                .Where(transaction => transaction.Type == TransactionType.Expense)
                .Sum(transaction => transaction.Amount);

            var projectedRecurringExpense = recurringRules.Sum(rule =>
                RecurringDateCalculator.GetOccurrences(
                        rule.Frequency,
                        rule.StartDate,
                        rule.EndDate,
                        rule.DayOfMonth,
                        monthEnd)
                    .Where(date => date >= monthStart)
                    .Where(date => monthTransactions.All(transaction =>
                        transaction.RecurringRuleId != rule.Id || transaction.Date != date))
                    .Sum(_ => rule.Amount));

            var totalExpense = expense + projectedRecurringExpense;
            result.Add(new ForecastMonthDto(
                monthDate.Year,
                monthDate.Month,
                income,
                totalExpense,
                income - totalExpense,
                projectedRecurringExpense));
        }

        return result;
    }
}
