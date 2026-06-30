using FinTrack.Domain.Enums;

namespace FinTrack.Application.Imports;

public sealed record CsvImportPreviewDto(
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    IReadOnlyList<CsvImportRowPreviewDto> Rows);

public sealed record CsvImportRowPreviewDto(
    int RowNumber,
    string Description,
    decimal? Amount,
    TransactionType? Type,
    DateOnly? Date,
    Guid? AccountId,
    Guid? CategoryId,
    DateOnly? DueDate,
    bool? IsPaid,
    DateOnly? PaymentDate,
    string? ImportHash,
    IReadOnlyList<string> Errors);

public sealed record ImportBatchDto(
    Guid Id,
    string FileName,
    FileImportType FileType,
    DateTime ImportedAt,
    int TotalRows,
    int SuccessRows,
    int FailedRows,
    FileImportStatus Status);
