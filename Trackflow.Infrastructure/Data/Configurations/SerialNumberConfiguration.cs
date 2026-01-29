using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackflow.Domain.Entities;

namespace Trackflow.Infrastructure.Data.Configurations;

public class SerialNumberConfiguration : IEntityTypeConfiguration<SerialNumber>
{
    public void Configure(EntityTypeBuilder<SerialNumber> builder)
    {
        builder.ToTable("SerialNumbers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeriNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.GS1Barkod)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(s => s.SeriNo)
            .IsUnique();

        builder.HasOne(s => s.WorkOrder)
            .WithMany(w => w.SerialNumbers)
            .HasForeignKey(s => s.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PackingUnit)
            .WithMany(p => p.SerialNumbers)
            .HasForeignKey(s => s.PackingUnitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
