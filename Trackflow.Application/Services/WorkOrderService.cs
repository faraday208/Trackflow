using Microsoft.EntityFrameworkCore;
using Trackflow.Application.GS1;
using Trackflow.Domain.Entities;
using Trackflow.Infrastructure.Data;
using Trackflow.Shared.DTOs;
using Trackflow.Shared.Enums;

namespace Trackflow.Application.Services;

public class WorkOrderService
{
    private readonly AppDbContext _context;
    private readonly GS1Service _gs1Service;

    public WorkOrderService(AppDbContext context, GS1Service gs1Service)
    {
        _context = context;
        _gs1Service = gs1Service;
    }

    public async Task<List<WorkOrderDto>> GetAllAsync()
    {
        return await _context.WorkOrders
            .Include(w => w.Product)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.UrunAdi,
                GTIN = w.Product.GTIN,
                Miktar = w.Miktar,
                LotNo = w.LotNo,
                SonKullanmaTarihi = w.SonKullanmaTarihi,
                Durum = w.Durum,
                KoliKapasitesi = w.KoliKapasitesi,
                PaletKapasitesi = w.PaletKapasitesi,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<WorkOrderDto?> GetByIdAsync(Guid id)
    {
        return await _context.WorkOrders
            .Include(w => w.Product)
            .Where(w => w.Id == id)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.UrunAdi,
                GTIN = w.Product.GTIN,
                Miktar = w.Miktar,
                LotNo = w.LotNo,
                SonKullanmaTarihi = w.SonKullanmaTarihi,
                Durum = w.Durum,
                KoliKapasitesi = w.KoliKapasitesi,
                PaletKapasitesi = w.PaletKapasitesi,
                CreatedAt = w.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<WorkOrderDetailDto?> GetDetailAsync(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .Include(w => w.SerialNumbers)
            .Include(w => w.PackingUnits)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
            return null;

        var dto = new WorkOrderDetailDto
        {
            Id = workOrder.Id,
            Miktar = workOrder.Miktar,
            LotNo = workOrder.LotNo,
            SonKullanmaTarihi = workOrder.SonKullanmaTarihi,
            Durum = workOrder.Durum,
            KoliKapasitesi = workOrder.KoliKapasitesi,
            PaletKapasitesi = workOrder.PaletKapasitesi,
            CreatedAt = workOrder.CreatedAt,
            Product = new ProductDto
            {
                Id = workOrder.Product.Id,
                CustomerId = workOrder.Product.CustomerId,
                CustomerName = workOrder.Product.Customer.FirmaAdi,
                GTIN = workOrder.Product.GTIN,
                UrunAdi = workOrder.Product.UrunAdi,
                CreatedAt = workOrder.Product.CreatedAt
            },
            Customer = new CustomerDto
            {
                Id = workOrder.Product.Customer.Id,
                FirmaAdi = workOrder.Product.Customer.FirmaAdi,
                GLN = workOrder.Product.Customer.GLN,
                Aciklama = workOrder.Product.Customer.Aciklama,
                CreatedAt = workOrder.Product.Customer.CreatedAt
            },
            SerialNumbers = workOrder.SerialNumbers
                .OrderBy(s => s.SeriNo)
                .Select(s => new SerialNumberDto
                {
                    Id = s.Id,
                    SeriNo = s.SeriNo,
                    Durum = s.Durum,
                    GS1Barkod = s.GS1Barkod,
                    PackingUnitId = s.PackingUnitId
                })
                .ToList(),
            PackingUnits = workOrder.PackingUnits
                .Where(p => p.ParentId == null)
                .Select(p => MapPackingUnit(p, workOrder.PackingUnits.ToList()))
                .ToList(),
            TotalSerials = workOrder.SerialNumbers.Count,
            AggregatedSerials = workOrder.SerialNumbers.Count(s => s.PackingUnitId != null),
            TotalBoxes = workOrder.PackingUnits.Count(p => p.Tip == PackingUnitType.Box),
            TotalPallets = workOrder.PackingUnits.Count(p => p.Tip == PackingUnitType.Pallet)
        };

        return dto;
    }

    private PackingUnitDto MapPackingUnit(PackingUnit unit, List<PackingUnit> allUnits)
    {
        var children = allUnits.Where(u => u.ParentId == unit.Id).ToList();

        return new PackingUnitDto
        {
            Id = unit.Id,
            Tip = unit.Tip,
            SSCC = unit.SSCC,
            ParentId = unit.ParentId,
            ItemCount = unit.SerialNumbers?.Count ?? children.Sum(c => c.SerialNumbers?.Count ?? 0),
            Children = children.Select(c => MapPackingUnit(c, allUnits)).ToList()
        };
    }

    public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
            throw new ArgumentException("Product not found");

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            Miktar = dto.Miktar,
            LotNo = dto.LotNo,
            SonKullanmaTarihi = dto.SonKullanmaTarihi,
            SeriBaslangic = dto.SeriBaslangic,
            KoliKapasitesi = dto.KoliKapasitesi,
            PaletKapasitesi = dto.PaletKapasitesi,
            Durum = WorkOrderStatus.Created
        };

        _context.WorkOrders.Add(workOrder);

        // Seri numaralarını üret
        var serialNumbers = new List<SerialNumber>();
        for (int i = 0; i < dto.Miktar; i++)
        {
            var seriNo = (dto.SeriBaslangic + i).ToString().PadLeft(10, '0');
            var gs1Barkod = _gs1Service.GenerateGS1Barcode(
                product.GTIN,
                seriNo,
                dto.SonKullanmaTarihi,
                dto.LotNo
            );

            serialNumbers.Add(new SerialNumber
            {
                Id = Guid.NewGuid(),
                WorkOrderId = workOrder.Id,
                SeriNo = seriNo,
                GS1Barkod = gs1Barkod,
                Durum = SerialNumberStatus.Generated
            });
        }

        _context.SerialNumbers.AddRange(serialNumbers);
        await _context.SaveChangesAsync();

        return new WorkOrderDto
        {
            Id = workOrder.Id,
            ProductId = workOrder.ProductId,
            ProductName = product.UrunAdi,
            GTIN = product.GTIN,
            Miktar = workOrder.Miktar,
            LotNo = workOrder.LotNo,
            SonKullanmaTarihi = workOrder.SonKullanmaTarihi,
            Durum = workOrder.Durum,
            KoliKapasitesi = workOrder.KoliKapasitesi,
            PaletKapasitesi = workOrder.PaletKapasitesi,
            CreatedAt = workOrder.CreatedAt
        };
    }

    public async Task<bool> UpdateStatusAsync(Guid id, WorkOrderStatus status)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null)
            return false;

        workOrder.Durum = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .Include(w => w.PackingUnits)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
            return false;

        _context.SerialNumbers.RemoveRange(workOrder.SerialNumbers);
        _context.PackingUnits.RemoveRange(workOrder.PackingUnits);
        _context.WorkOrders.Remove(workOrder);
        await _context.SaveChangesAsync();
        return true;
    }
}
