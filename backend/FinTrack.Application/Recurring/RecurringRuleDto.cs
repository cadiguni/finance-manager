using FinTrack.Domain.Enums;

namespace FinTrack.Application.Recurring;

public sealed record RecurringRuleDto(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    string Description,
    decimal Amount,
    RecurringFrequency Frequency,
    int DayOfMonth,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive);
