using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categorization;

public sealed record CreateCategoryKeywordRuleRequest(
    Guid CategoryId,
    string Keyword,
    TransactionType? TransactionType,
    int Priority,
    bool IsActive);

public sealed record UpdateCategoryKeywordRuleRequest(
    Guid CategoryId,
    string Keyword,
    TransactionType? TransactionType,
    int Priority,
    bool IsActive);
