using FinTrack.Application.Imports;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Imports;

public sealed class ImportBatchRepository : IImportBatchRepository
{
    private readonly FinTrackDbContext _dbContext;

    public ImportBatchRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ImportBatch>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ImportBatches
            .AsNoTracking()
            .Where(batch => batch.UserId == userId)
            .OrderByDescending(batch => batch.ImportedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByContentHashAsync(
        Guid userId,
        string contentHash,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ImportBatches
            .AnyAsync(
                batch => batch.UserId == userId && batch.ContentHash == contentHash,
                cancellationToken);
    }

    public async Task AddAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        await _dbContext.ImportBatches.AddAsync(batch, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
