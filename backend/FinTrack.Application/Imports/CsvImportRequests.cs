namespace FinTrack.Application.Imports;

public sealed record CsvPreviewRequest(string FileName, string Content);

public sealed record CommitCsvImportRequest(string FileName, string Content);
