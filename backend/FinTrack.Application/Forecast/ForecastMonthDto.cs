namespace FinTrack.Application.Forecast;

public sealed record ForecastMonthDto(
    int Year,
    int Month,
    decimal Income,
    decimal Expense,
    decimal Balance,
    decimal ProjectedRecurringExpenses);
