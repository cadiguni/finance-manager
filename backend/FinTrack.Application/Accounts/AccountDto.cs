using FinTrack.Domain.Enums;

namespace FinTrack.Application.Accounts;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    DateTime CreatedAt);
