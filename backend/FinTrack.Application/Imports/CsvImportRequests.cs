namespace FinTrack.Application.Imports;

public sealed record CsvPreviewRequest(string FileName, string Content);

public sealed record CommitCsvImportRequest(string FileName, string Content);

public sealed record ExcelPreviewRequest(string FileName, string ContentBase64, string? WorksheetName);

public sealed record CommitExcelImportRequest(string FileName, string ContentBase64, string? WorksheetName);
