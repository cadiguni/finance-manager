using FinTrack.Application.Common;

namespace FinTrack.Application.Dashboard;

public interface IDashboardService
{
    Task<Result<MonthlySummaryDto>> GetMonthlySummaryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken);
}
