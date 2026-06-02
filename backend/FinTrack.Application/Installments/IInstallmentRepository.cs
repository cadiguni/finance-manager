using FinTrack.Domain.Entities;

namespace FinTrack.Application.Installments;

public interface IInstallmentRepository
{
    Task AddAsync(InstallmentGroup group, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
