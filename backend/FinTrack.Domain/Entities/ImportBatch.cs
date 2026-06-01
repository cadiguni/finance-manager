using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class ImportBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public FileImportType FileType { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public FileImportStatus Status { get; set; } = FileImportStatus.Pending;

    public User? User { get; set; }
}
