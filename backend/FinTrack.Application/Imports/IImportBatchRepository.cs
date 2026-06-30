using FinTrack.Domain.Entities;

namespace FinTrack.Application.Imports;

public interface IImportBatchRepository
{
    Task<IReadOnlyList<ImportBatch>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsByContentHashAsync(
        Guid userId,
        string contentHash,
        CancellationToken cancellationToken);
    Task AddAsync(ImportBatch batch, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
