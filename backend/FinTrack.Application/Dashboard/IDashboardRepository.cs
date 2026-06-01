namespace FinTrack.Application.Dashboard;

public interface IDashboardRepository
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken);
}
