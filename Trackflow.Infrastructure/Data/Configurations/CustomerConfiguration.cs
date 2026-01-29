using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackflow.Domain.Entities;

namespace Trackflow.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirmaAdi)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.GLN)
            .IsRequired()
            .HasMaxLength(13);

        builder.Property(c => c.Aciklama)
            .HasMaxLength(500);

        builder.HasIndex(c => c.GLN)
            .IsUnique();
    }
}
