using FinTrack.Application.Installments;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;

namespace FinTrack.Infrastructure.Installments;

public sealed class InstallmentRepository : IInstallmentRepository
{
    private readonly FinTrackDbContext _dbContext;

    public InstallmentRepository(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(InstallmentGroup group, CancellationToken cancellationToken)
    {
        await _dbContext.InstallmentGroups.AddAsync(group, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
