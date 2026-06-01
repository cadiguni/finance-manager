namespace FinTrack.Application.Dashboard;

public sealed record CategorySummaryDto(
    Guid CategoryId,
    string CategoryName,
    decimal Total);
