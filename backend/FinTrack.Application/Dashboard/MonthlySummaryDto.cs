namespace FinTrack.Application.Dashboard;

public sealed record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    IReadOnlyList<CategorySummaryDto> ExpensesByCategory,
    IReadOnlyList<CategorySummaryDto> IncomeByCategory,
    decimal UpcomingPayments,
    decimal PaidExpenses,
    decimal UnpaidExpenses);
