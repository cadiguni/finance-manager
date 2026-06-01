using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class InstallmentGroupConfiguration : IEntityTypeConfiguration<InstallmentGroup>
{
    public void Configure(EntityTypeBuilder<InstallmentGroup> builder)
    {
        builder.HasKey(group => group.Id);

        builder.Property(group => group.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(group => group.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(group => group.InstallmentAmount)
            .HasPrecision(18, 2);

        builder.HasOne(group => group.User)
            .WithMany()
            .HasForeignKey(group => group.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
