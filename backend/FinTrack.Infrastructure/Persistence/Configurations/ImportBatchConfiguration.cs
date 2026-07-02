using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.HasKey(batch => batch.Id);

        builder.Property(batch => batch.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(batch => batch.ContentHash)
            .HasMaxLength(64);

        builder.Property(batch => batch.IncomeAmount).HasPrecision(18, 2);
        builder.Property(batch => batch.ExpenseAmount).HasPrecision(18, 2);

        builder.HasIndex(batch => new { batch.UserId, batch.ContentHash })
            .IsUnique();

        builder.Property(batch => batch.FileType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(batch => batch.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(batch => batch.User)
            .WithMany()
            .HasForeignKey(batch => batch.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
