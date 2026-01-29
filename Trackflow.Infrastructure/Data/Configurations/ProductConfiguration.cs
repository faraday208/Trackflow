using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackflow.Domain.Entities;

namespace Trackflow.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.GTIN)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(p => p.UrunAdi)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.GTIN)
            .IsUnique();

        builder.HasOne(p => p.Customer)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
