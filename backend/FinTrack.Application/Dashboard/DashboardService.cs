using FinTrack.Application.Common;

namespace FinTrack.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<MonthlySummaryDto>> GetMonthlySummaryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        if (year < 2000 || year > 2100)
        {
            return Result<MonthlySummaryDto>.Failure("Year must be between 2000 and 2100.");
        }

        if (month < 1 || month > 12)
        {
            return Result<MonthlySummaryDto>.Failure("Month must be between 1 and 12.");
        }

        var summary = await _repository.GetMonthlySummaryAsync(userId, year, month, cancellationToken);

        return Result<MonthlySummaryDto>.Success(summary);
    }
}
