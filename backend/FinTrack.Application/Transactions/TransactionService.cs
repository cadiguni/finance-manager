using FinTrack.Application.Accounts;
using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAllAsync(
        Guid userId,
        TransactionFilters filters,
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetAllAsync(userId, filters, cancellationToken);

        return transactions
            .Select(MapToDto)
            .ToList();
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(userId, id, cancellationToken);

        return transaction is null ? null : MapToDto(transaction);
    }

    public async Task<Result<TransactionDto>> CreateAsync(
        Guid userId,
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateAsync(
            userId,
            request.AccountId,
            request.CategoryId,
            request.Description,
            request.Amount,
            request.Type,
            request.IsPaid,
            request.PaymentDate,
            cancellationToken);

        if (validationError is not null)
        {
            return Result<TransactionDto>.Failure(validationError);
        }

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            Date = request.Date,
            DueDate = request.DueDate,
            IsPaid = request.IsPaid,
            PaymentDate = request.PaymentDate
        };

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return Result<TransactionDto>.Success(MapToDto(transaction));
    }

    public async Task<Result> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(userId, id, cancellationToken);
        if (transaction is null)
        {
            return Result.Failure("Transaction not found.");
        }

        var validationError = await ValidateAsync(
            userId,
            request.AccountId,
            request.CategoryId,
            request.Description,
            request.Amount,
            request.Type,
            request.IsPaid,
            request.PaymentDate,
            cancellationToken);

        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        transaction.AccountId = request.AccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.Description = request.Description.Trim();
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Date = request.Date;
        transaction.DueDate = request.DueDate;
        transaction.IsPaid = request.IsPaid;
        transaction.PaymentDate = request.PaymentDate;

        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(userId, id, cancellationToken);
        if (transaction is null)
        {
            return Result.Failure("Transaction not found.");
        }

        _transactionRepository.Remove(transaction);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<string?> ValidateAsync(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        string description,
        decimal amount,
        TransactionType transactionType,
        bool isPaid,
        DateOnly? paymentDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Transaction description is required.";
        }

        if (amount <= 0)
        {
            return "Transaction amount must be greater than zero.";
        }

        if (isPaid && paymentDate is null)
        {
            return "Payment date is required when the transaction is paid.";
        }

        if (!isPaid && paymentDate is not null)
        {
            return "Payment date must be empty when the transaction is pending.";
        }

        var account = await _accountRepository.GetByIdAsync(userId, accountId, cancellationToken);
        if (account is null)
        {
            return "Account not found.";
        }

        var category = await _categoryRepository.GetByIdAsync(userId, categoryId, cancellationToken);
        if (category is null)
        {
            return "Category not found.";
        }

        if (category.Type != CategoryType.Both &&
            (transactionType == TransactionType.Income && category.Type != CategoryType.Income ||
             transactionType == TransactionType.Expense && category.Type != CategoryType.Expense))
        {
            return "Category type is incompatible with transaction type.";
        }

        return null;
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            transaction.CategoryId,
            transaction.Description,
            transaction.Amount,
            transaction.Type,
            transaction.Date,
            transaction.DueDate,
            transaction.IsPaid,
            transaction.PaymentDate,
            transaction.InstallmentGroupId,
            transaction.RecurringRuleId,
            transaction.CreatedAt);
    }
}
