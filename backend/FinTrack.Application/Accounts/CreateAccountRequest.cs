using FinTrack.Domain.Enums;

namespace FinTrack.Application.Accounts;

public sealed record CreateAccountRequest(
    string Name,
    AccountType Type,
    decimal InitialBalance);
