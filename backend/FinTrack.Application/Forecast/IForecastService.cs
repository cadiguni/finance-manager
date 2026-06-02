namespace FinTrack.Application.Forecast;

public interface IForecastService
{
    Task<IReadOnlyList<ForecastMonthDto>> GetNextMonthsAsync(
        Guid userId,
        int startYear,
        int startMonth,
        int months,
        CancellationToken cancellationToken);
}
