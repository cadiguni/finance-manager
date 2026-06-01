using FinTrack.Domain.Enums;

namespace FinTrack.Application.Accounts;

public sealed record UpdateAccountRequest(
    string Name,
    AccountType Type,
    decimal InitialBalance);
