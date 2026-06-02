using FinTrack.Application.Common;

namespace FinTrack.Application.Recurring;

public interface IRecurringRuleService
{
    Task<IReadOnlyList<RecurringRuleDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<RecurringRuleDto>> CreateAsync(
        Guid userId,
        CreateRecurringRuleRequest request,
        CancellationToken cancellationToken);

    Task<Result<GenerateRecurringTransactionsResult>> GenerateAsync(
        Guid userId,
        GenerateRecurringTransactionsRequest request,
        CancellationToken cancellationToken);
}
