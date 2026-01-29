using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackflow.Domain.Entities;

namespace Trackflow.Infrastructure.Data.Configurations;

public class PackingUnitConfiguration : IEntityTypeConfiguration<PackingUnit>
{
    public void Configure(EntityTypeBuilder<PackingUnit> builder)
    {
        builder.ToTable("PackingUnits");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SSCC)
            .IsRequired()
            .HasMaxLength(18);

        builder.HasIndex(p => p.SSCC)
            .IsUnique();

        builder.HasOne(p => p.WorkOrder)
            .WithMany(w => w.PackingUnits)
            .HasForeignKey(p => p.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
