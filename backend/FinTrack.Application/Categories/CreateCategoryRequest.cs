using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    CategoryType Type,
    Guid? ParentCategoryId);
