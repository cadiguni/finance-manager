using FinTrack.Domain.Entities;

namespace FinTrack.Application.Recurring;

public interface IRecurringRuleRepository
{
    Task<IReadOnlyList<RecurringRule>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RecurringRule>> GetActiveAsync(Guid userId, CancellationToken cancellationToken);
    Task<RecurringRule?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(RecurringRule rule, CancellationToken cancellationToken);
    Task<bool> HasTransactionAsync(Guid userId, Guid ruleId, DateOnly date, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
