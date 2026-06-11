namespace FinTrack.Application.Imports;

public sealed record CsvPreviewRequest(
    string FileName,
    string Content,
    Guid? DefaultAccountId = null,
    Guid? DefaultCategoryId = null);

public sealed record CommitCsvImportRequest(
    string FileName,
    string Content,
    Guid? DefaultAccountId = null,
    Guid? DefaultCategoryId = null);

public sealed record ExcelPreviewRequest(string FileName, string ContentBase64, string? WorksheetName);

public sealed record CommitExcelImportRequest(string FileName, string ContentBase64, string? WorksheetName);

public sealed record CardStatementPreviewRequest(
    string FileName,
    string Content,
    string? ContentBase64,
    Guid AccountId,
    DateOnly DueDate,
    bool IsPaid,
    DateOnly? PaymentDate);

public sealed record CommitCardStatementImportRequest(
    string FileName,
    string Content,
    string? ContentBase64,
    Guid AccountId,
    DateOnly DueDate,
    bool IsPaid,
    DateOnly? PaymentDate);
