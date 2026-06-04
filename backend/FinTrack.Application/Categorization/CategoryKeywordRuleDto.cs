using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categorization;

public sealed record CategoryKeywordRuleDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Keyword,
    TransactionType? TransactionType,
    int Priority,
    bool IsActive,
    DateTime CreatedAt);
