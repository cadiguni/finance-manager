using FinTrack.Application.Accounts;
using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Installments;

public sealed class InstallmentService : IInstallmentService
{
    private readonly IInstallmentRepository _installmentRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public InstallmentService(
        IInstallmentRepository installmentRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _installmentRepository = installmentRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<InstallmentGroupDto>> CreatePurchaseAsync(
        Guid userId,
        CreateInstallmentPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateAsync(userId, request, cancellationToken);
        if (validationError is not null)
        {
            return Result<InstallmentGroupDto>.Failure(validationError);
        }

        var installmentAmount = decimal.Round(request.TotalAmount / request.TotalInstallments, 2);
        var group = new InstallmentGroup
        {
            UserId = userId,
            Description = request.Description.Trim(),
            TotalAmount = request.TotalAmount,
            InstallmentAmount = installmentAmount,
            TotalInstallments = request.TotalInstallments,
            StartDate = request.StartDate
        };

        var allocated = 0m;
        for (var index = 1; index <= request.TotalInstallments; index++)
        {
            var amount = index == request.TotalInstallments
                ? request.TotalAmount - allocated
                : installmentAmount;
            allocated += amount;

            var date = AddMonthsKeepingDay(request.StartDate, index - 1, request.DueDay);
            group.Transactions.Add(new Transaction
            {
                UserId = userId,
                AccountId = request.AccountId,
                CategoryId = request.CategoryId,
                Description = $"{group.Description} ({index}/{request.TotalInstallments})",
                Amount = amount,
                Type = TransactionType.Expense,
                Date = date,
                DueDate = date,
                IsPaid = false,
                InstallmentGroupId = group.Id
            });
        }

        await _installmentRepository.AddAsync(group, cancellationToken);
        await _installmentRepository.SaveChangesAsync(cancellationToken);

        return Result<InstallmentGroupDto>.Success(MapToDto(group));
    }

    private async Task<string?> ValidateAsync(
        Guid userId,
        CreateInstallmentPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "Installment purchase description is required.";
        }

        if (request.TotalAmount <= 0)
        {
            return "Installment purchase amount must be greater than zero.";
        }

        if (request.TotalInstallments < 2)
        {
            return "Installment purchase must have at least two installments.";
        }

        if (request.DueDay is < 1 or > 31)
        {
            return "Due day must be between 1 and 31.";
        }

        var account = await _accountRepository.GetByIdAsync(userId, request.AccountId, cancellationToken);
        if (account is null)
        {
            return "Account not found.";
        }

        var category = await _categoryRepository.GetByIdAsync(userId, request.CategoryId, cancellationToken);
        if (category is null)
        {
            return "Category not found.";
        }

        if (category.Type == CategoryType.Income)
        {
            return "Installment purchases require an expense category.";
        }

        return null;
    }

    private static DateOnly AddMonthsKeepingDay(DateOnly startDate, int months, int? dueDay)
    {
        var month = startDate.AddMonths(months);
        var day = dueDay ?? startDate.Day;
        var safeDay = Math.Min(day, DateTime.DaysInMonth(month.Year, month.Month));
        return new DateOnly(month.Year, month.Month, safeDay);
    }

    private static InstallmentGroupDto MapToDto(InstallmentGroup group)
    {
        return new InstallmentGroupDto(
            group.Id,
            group.Description,
            group.TotalAmount,
            group.InstallmentAmount,
            group.TotalInstallments,
            group.StartDate,
            group.CreatedAt);
    }
}
