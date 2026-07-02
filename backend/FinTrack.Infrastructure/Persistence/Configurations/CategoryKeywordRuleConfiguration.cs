using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
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

        var createdAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var rules = new (string Keyword, Guid CategoryId)[]
        {
            ("ifood", DemoSeedIds.FoodCategoryId),
            ("restaurante", DemoSeedIds.FoodCategoryId),
            ("pizzaria", DemoSeedIds.FoodCategoryId),
            ("macromix", DemoSeedIds.FoodCategoryId),
            ("fardo", DemoSeedIds.FoodCategoryId),
            ("aliment", DemoSeedIds.FoodCategoryId),
            ("crunchyroll", DemoSeedIds.EntertainmentCategoryId),
            ("youtube", DemoSeedIds.EntertainmentCategoryId),
            ("spotify", DemoSeedIds.EntertainmentCategoryId),
            ("amazonprime", DemoSeedIds.EntertainmentCategoryId),
            ("steam", DemoSeedIds.EntertainmentCategoryId),
            ("nuuvem", DemoSeedIds.EntertainmentCategoryId),
            ("wellhub", DemoSeedIds.HealthCategoryId),
            ("seguro", DemoSeedIds.InsuranceCategoryId),
            ("uninter", DemoSeedIds.EducationCategoryId),
            ("mercadolivre", DemoSeedIds.ShoppingCategoryId),
            ("mercado*", DemoSeedIds.ShoppingCategoryId),
            ("amazon", DemoSeedIds.ShoppingCategoryId),
            ("modas", DemoSeedIds.ShoppingCategoryId),
            ("prata fina", DemoSeedIds.ShoppingCategoryId),
            ("floricultura", DemoSeedIds.ShoppingCategoryId),
            ("shop ", DemoSeedIds.ShoppingCategoryId)
        };

        builder.HasData(rules.Select((rule, index) => new CategoryKeywordRule
        {
            Id = Guid.Parse($"55555555-5555-5555-5555-{index + 1:000000000000}"),
            UserId = DemoSeedIds.UserId,
            CategoryId = rule.CategoryId,
            Keyword = rule.Keyword,
            TransactionType = TransactionType.Expense,
            Priority = 100,
            IsActive = true,
            CreatedAt = createdAt
        }));
    }
}
