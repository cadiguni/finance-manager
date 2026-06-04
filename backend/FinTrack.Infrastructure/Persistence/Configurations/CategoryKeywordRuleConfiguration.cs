using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class CategoryKeywordRuleConfiguration : IEntityTypeConfiguration<CategoryKeywordRule>
{
    public void Configure(EntityTypeBuilder<CategoryKeywordRule> builder)
    {
        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.Keyword)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(rule => rule.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(rule => new { rule.UserId, rule.Keyword, rule.TransactionType })
            .IsUnique();

        builder.HasOne(rule => rule.User)
            .WithMany()
            .HasForeignKey(rule => rule.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rule => rule.Category)
            .WithMany()
            .HasForeignKey(rule => rule.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
