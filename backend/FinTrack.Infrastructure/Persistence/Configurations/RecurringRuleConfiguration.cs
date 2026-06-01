using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class RecurringRuleConfiguration : IEntityTypeConfiguration<RecurringRule>
{
    public void Configure(EntityTypeBuilder<RecurringRule> builder)
    {
        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(rule => rule.Amount)
            .HasPrecision(18, 2);

        builder.Property(rule => rule.Frequency)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(rule => rule.User)
            .WithMany()
            .HasForeignKey(rule => rule.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rule => rule.Category)
            .WithMany()
            .HasForeignKey(rule => rule.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rule => rule.Account)
            .WithMany()
            .HasForeignKey(rule => rule.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
