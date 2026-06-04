using FinTrack.Application.Common;

namespace FinTrack.Application.Imports;

public interface IImportService
{
    Task<CsvImportPreviewDto> PreviewCsvAsync(
        Guid userId,
        CsvPreviewRequest request,
        CancellationToken cancellationToken);

    Task<Result<ImportBatchDto>> CommitCsvAsync(
        Guid userId,
        CommitCsvImportRequest request,
        CancellationToken cancellationToken);

    Task<CsvImportPreviewDto> PreviewExcelAsync(
        Guid userId,
        ExcelPreviewRequest request,
        CancellationToken cancellationToken);

    Task<Result<ImportBatchDto>> CommitExcelAsync(
        Guid userId,
        CommitExcelImportRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ImportBatchDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
}
