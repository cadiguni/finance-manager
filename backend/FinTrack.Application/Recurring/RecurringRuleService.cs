using FinTrack.Application.Accounts;
using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Recurring;

public sealed class RecurringRuleService : IRecurringRuleService
{
    private readonly IRecurringRuleRepository _recurringRuleRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public RecurringRuleService(
        IRecurringRuleRepository recurringRuleRepository,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _recurringRuleRepository = recurringRuleRepository;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<RecurringRuleDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rules = await _recurringRuleRepository.GetAllAsync(userId, cancellationToken);
        return rules.Select(MapToDto).ToList();
    }

    public async Task<Result<RecurringRuleDto>> CreateAsync(
        Guid userId,
        CreateRecurringRuleRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateAsync(userId, request, cancellationToken);
        if (validationError is not null)
        {
            return Result<RecurringRuleDto>.Failure(validationError);
        }

        var rule = new RecurringRule
        {
            UserId = userId,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Frequency = request.Frequency,
            DayOfMonth = request.DayOfMonth,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        await _recurringRuleRepository.AddAsync(rule, cancellationToken);
        await _recurringRuleRepository.SaveChangesAsync(cancellationToken);

        return Result<RecurringRuleDto>.Success(MapToDto(rule));
    }

    public async Task<Result<GenerateRecurringTransactionsResult>> GenerateAsync(
        Guid userId,
        GenerateRecurringTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        var rules = await _recurringRuleRepository.GetActiveAsync(userId, cancellationToken);
        var created = 0;

        foreach (var rule in rules)
        {
            var dates = RecurringDateCalculator.GetOccurrences(
                rule.Frequency,
                rule.StartDate,
                rule.EndDate,
                rule.DayOfMonth,
                request.ThroughDate);

            foreach (var date in dates)
            {
                var exists = await _recurringRuleRepository.HasTransactionAsync(
                    userId,
                    rule.Id,
                    date,
                    cancellationToken);
                if (exists)
                {
                    continue;
                }

                await _transactionRepository.AddAsync(new Transaction
                {
                    UserId = userId,
                    AccountId = rule.AccountId,
                    CategoryId = rule.CategoryId,
                    Description = rule.Description,
                    Amount = rule.Amount,
                    Type = TransactionType.Expense,
                    Date = date,
                    DueDate = date,
                    IsPaid = false,
                    RecurringRuleId = rule.Id
                }, cancellationToken);
                created++;
            }
        }

        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return Result<GenerateRecurringTransactionsResult>.Success(
            new GenerateRecurringTransactionsResult(created));
    }

    private async Task<string?> ValidateAsync(
        Guid userId,
        CreateRecurringRuleRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "Recurring expense description is required.";
        }

        if (request.Amount <= 0)
        {
            return "Recurring expense amount must be greater than zero.";
        }

        if (request.DayOfMonth is < 1 or > 31)
        {
            return "Day of month must be between 1 and 31.";
        }

        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            return "End date must be greater than or equal to start date.";
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
            return "Recurring expenses require an expense category.";
        }

        return null;
    }

    private static RecurringRuleDto MapToDto(RecurringRule rule)
    {
        return new RecurringRuleDto(
            rule.Id,
            rule.AccountId,
            rule.CategoryId,
            rule.Description,
            rule.Amount,
            rule.Frequency,
            rule.DayOfMonth,
            rule.StartDate,
            rule.EndDate,
            rule.IsActive);
    }
}
