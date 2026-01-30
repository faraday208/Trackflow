using Microsoft.EntityFrameworkCore;
using Trackflow.Domain.Entities;
using Trackflow.Shared.Enums;

namespace Trackflow.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<SerialNumber> SerialNumbers => Set<SerialNumber>();
    public DbSet<PackingUnit> PackingUnits => Set<PackingUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        SeedData(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var product1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var product2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var workOrderId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Customer>().HasData(new Customer
        {
            Id = customerId,
            FirmaAdi = "Demo Firma A.Ş.",
            GLN = "8690000000001",
            Aciklama = "Demo müşteri",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = product1Id,
                CustomerId = customerId,
                GTIN = "08690000000011",
                UrunAdi = "Test Ürün 1",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = product2Id,
                CustomerId = customerId,
                GTIN = "08690000000028",
                UrunAdi = "Test Ürün 2",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<WorkOrder>().HasData(new WorkOrder
        {
            Id = workOrderId,
            ProductId = product1Id,
            Miktar = 100,
            LotNo = "LOT001",
            SonKullanmaTarihi = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            SeriBaslangic = 1,
            Durum = WorkOrderStatus.Created,
            KoliKapasitesi = 10,
            PaletKapasitesi = 10,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
