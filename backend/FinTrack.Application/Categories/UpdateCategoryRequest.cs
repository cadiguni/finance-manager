using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    CategoryType Type,
    Guid? ParentCategoryId);
