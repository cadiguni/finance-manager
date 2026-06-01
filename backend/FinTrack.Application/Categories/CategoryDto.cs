using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    CategoryType Type,
    Guid? ParentCategoryId,
    DateTime CreatedAt);
