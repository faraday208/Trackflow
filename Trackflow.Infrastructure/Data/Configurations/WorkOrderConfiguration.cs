using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackflow.Domain.Entities;

namespace Trackflow.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.LotNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Miktar)
            .IsRequired();

        builder.Property(w => w.KoliKapasitesi)
            .HasDefaultValue(10);

        builder.Property(w => w.PaletKapasitesi)
            .HasDefaultValue(10);

        builder.HasOne(w => w.Product)
            .WithMany(p => p.WorkOrders)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
