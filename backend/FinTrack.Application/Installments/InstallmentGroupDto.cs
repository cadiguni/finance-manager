namespace FinTrack.Application.Installments;

public sealed record InstallmentGroupDto(
    Guid Id,
    string Description,
    decimal TotalAmount,
    decimal InstallmentAmount,
    int TotalInstallments,
    DateOnly StartDate,
    DateTime CreatedAt);
