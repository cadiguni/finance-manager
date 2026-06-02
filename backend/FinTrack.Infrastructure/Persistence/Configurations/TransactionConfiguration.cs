using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasPrecision(18, 2);

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(transaction => transaction.User)
            .WithMany()
            .HasForeignKey(transaction => transaction.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(transaction => transaction.Account)
            .WithMany()
            .HasForeignKey(transaction => transaction.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.Category)
            .WithMany()
            .HasForeignKey(transaction => transaction.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.InstallmentGroup)
            .WithMany(group => group.Transactions)
            .HasForeignKey(transaction => transaction.InstallmentGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(transaction => transaction.RecurringRule)
            .WithMany(rule => rule.Transactions)
            .HasForeignKey(transaction => transaction.RecurringRuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasData(
            new Transaction
            {
                Id = DemoSeedIds.SalaryTransactionId,
                UserId = DemoSeedIds.UserId,
                AccountId = DemoSeedIds.MainAccountId,
                CategoryId = DemoSeedIds.SalaryCategoryId,
                Description = "Salario mensal",
                Amount = 6500m,
                Type = Domain.Enums.TransactionType.Income,
                Date = new DateOnly(2026, 6, 5),
                IsPaid = true,
                PaymentDate = new DateOnly(2026, 6, 5),
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = DemoSeedIds.GroceriesTransactionId,
                UserId = DemoSeedIds.UserId,
                AccountId = DemoSeedIds.MainAccountId,
                CategoryId = DemoSeedIds.FoodCategoryId,
                Description = "Mercado",
                Amount = 420.75m,
                Type = Domain.Enums.TransactionType.Expense,
                Date = new DateOnly(2026, 6, 7),
                DueDate = new DateOnly(2026, 6, 7),
                IsPaid = true,
                PaymentDate = new DateOnly(2026, 6, 7),
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = DemoSeedIds.RentTransactionId,
                UserId = DemoSeedIds.UserId,
                AccountId = DemoSeedIds.MainAccountId,
                CategoryId = DemoSeedIds.HousingCategoryId,
                Description = "Aluguel",
                Amount = 1800m,
                Type = Domain.Enums.TransactionType.Expense,
                Date = new DateOnly(2026, 6, 10),
                DueDate = new DateOnly(2026, 6, 10),
                IsPaid = false,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = DemoSeedIds.TransportTransactionId,
                UserId = DemoSeedIds.UserId,
                AccountId = DemoSeedIds.CashAccountId,
                CategoryId = DemoSeedIds.TransportCategoryId,
                Description = "Transporte semanal",
                Amount = 85.50m,
                Type = Domain.Enums.TransactionType.Expense,
                Date = new DateOnly(2026, 6, 12),
                DueDate = new DateOnly(2026, 6, 12),
                IsPaid = false,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
