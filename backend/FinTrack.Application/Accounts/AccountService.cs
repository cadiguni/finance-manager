using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Accounts;

public sealed class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetAllAsync(userId, cancellationToken);

        return accounts
            .Select(MapToDto)
            .ToList();
    }

    public async Task<AccountDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(userId, id, cancellationToken);

        return account is null ? null : MapToDto(account);
    }

    public async Task<Result<AccountDto>> CreateAsync(
        Guid userId,
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request.Name);
        if (validationError is not null)
        {
            return Result<AccountDto>.Failure(validationError);
        }

        var account = new Account
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type,
            InitialBalance = request.InitialBalance
        };

        await _repository.AddAsync(account, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<AccountDto>.Success(MapToDto(account));
    }

    public async Task<Result> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(userId, id, cancellationToken);
        if (account is null)
        {
            return Result.Failure("Account not found.");
        }

        var validationError = Validate(request.Name);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        account.Name = request.Name.Trim();
        account.Type = request.Type;
        account.InitialBalance = request.InitialBalance;

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(userId, id, cancellationToken);
        if (account is null)
        {
            return Result.Failure("Account not found.");
        }

        if (await _repository.HasTransactionsAsync(userId, id, cancellationToken) ||
            await _repository.HasRecurringRulesAsync(userId, id, cancellationToken))
        {
            return Result.Failure(
                "Não é possível excluir esta conta porque ela está sendo usada em transações ou recorrências.");
        }

        _repository.Remove(account);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string? Validate(string name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? "Account name is required."
            : null;
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto(
            account.Id,
            account.Name,
            account.Type,
            account.InitialBalance,
            account.CreatedAt);
    }
}
