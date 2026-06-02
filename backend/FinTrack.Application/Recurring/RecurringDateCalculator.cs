using FinTrack.Domain.Enums;

namespace FinTrack.Application.Recurring;

public static class RecurringDateCalculator
{
    public static IReadOnlyList<DateOnly> GetOccurrences(
        RecurringFrequency frequency,
        DateOnly startDate,
        DateOnly? endDate,
        int dayOfMonth,
        DateOnly throughDate)
    {
        var finalDate = endDate.HasValue && endDate.Value < throughDate ? endDate.Value : throughDate;
        if (finalDate < startDate)
        {
            return Array.Empty<DateOnly>();
        }

        var dates = new List<DateOnly>();
        var current = NormalizeDate(startDate.Year, startDate.Month, dayOfMonth);
        if (current < startDate)
        {
            current = Next(frequency, current, dayOfMonth);
        }

        while (current <= finalDate)
        {
            dates.Add(current);
            current = Next(frequency, current, dayOfMonth);
        }

        return dates;
    }

    private static DateOnly Next(RecurringFrequency frequency, DateOnly current, int dayOfMonth)
    {
        return frequency switch
        {
            RecurringFrequency.Weekly => current.AddDays(7),
            RecurringFrequency.Yearly => NormalizeDate(current.Year + 1, current.Month, dayOfMonth),
            _ => NormalizeDate(current.AddMonths(1).Year, current.AddMonths(1).Month, dayOfMonth)
        };
    }

    private static DateOnly NormalizeDate(int year, int month, int dayOfMonth)
    {
        var safeDay = Math.Min(dayOfMonth, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, safeDay);
    }
}
