using FinTrack.Application.Common;

namespace FinTrack.Application.Accounts;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<AccountDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Result<AccountDto>> CreateAsync(Guid userId, CreateAccountRequest request, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(Guid userId, Guid id, UpdateAccountRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
