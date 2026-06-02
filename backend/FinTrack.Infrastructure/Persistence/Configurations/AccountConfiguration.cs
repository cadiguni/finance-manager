using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(account => account.Type)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(account => account.InitialBalance)
            .HasPrecision(18, 2);

        builder.HasOne(account => account.User)
            .WithMany()
            .HasForeignKey(account => account.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new Account
            {
                Id = DemoSeedIds.MainAccountId,
                UserId = DemoSeedIds.UserId,
                Name = "Conta Principal",
                Type = AccountType.BankAccount,
                InitialBalance = 2500m,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = DemoSeedIds.CashAccountId,
                UserId = DemoSeedIds.UserId,
                Name = "Carteira",
                Type = AccountType.Cash,
                InitialBalance = 300m,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
