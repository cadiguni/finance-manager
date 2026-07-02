using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(category => category.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(category => new { category.UserId, category.Name });

        builder.HasOne(category => category.User)
            .WithMany()
            .HasForeignKey(category => category.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(category => category.ParentCategory)
            .WithMany(category => category.Subcategories)
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Category
            {
                Id = DemoSeedIds.SalaryCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Salario",
                Type = CategoryType.Income,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.FoodCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Alimentacao",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.HousingCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Moradia",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.TransportCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Transporte",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.EntertainmentCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Assinaturas e entretenimento",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.HealthCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Saude",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.ShoppingCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Compras",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.EducationCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Educacao",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.InsuranceCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Seguros",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = DemoSeedIds.OtherExpensesCategoryId,
                UserId = DemoSeedIds.UserId,
                Name = "Outros",
                Type = CategoryType.Expense,
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
