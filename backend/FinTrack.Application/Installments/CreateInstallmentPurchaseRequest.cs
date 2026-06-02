namespace FinTrack.Application.Installments;

public sealed record CreateInstallmentPurchaseRequest(
    Guid AccountId,
    Guid CategoryId,
    string Description,
    decimal TotalAmount,
    int TotalInstallments,
    DateOnly StartDate,
    int? DueDay);
