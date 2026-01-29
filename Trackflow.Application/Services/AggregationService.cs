using Microsoft.EntityFrameworkCore;
using Trackflow.Application.DTOs;
using Trackflow.Application.GS1;
using Trackflow.Domain.Entities;
using Trackflow.Domain.Enums;
using Trackflow.Infrastructure.Data;

namespace Trackflow.Application.Services;

public class AggregationService
{
    private readonly AppDbContext _context;

    public AggregationService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// İş emri için otomatik agregasyon yapar
    /// - Önce seri numaralarını kolilere böler
    /// - Sonra kolileri paletlere böler
    /// </summary>
    public async Task<AggregationResultDto> AggregateWorkOrderAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .Include(w => w.SerialNumbers)
            .Include(w => w.PackingUnits)
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("Work order not found");

        // Mevcut agregasyonu temizle
        foreach (var serial in workOrder.SerialNumbers)
        {
            serial.PackingUnitId = null;
            serial.Durum = SerialNumberStatus.Generated;
        }
        _context.PackingUnits.RemoveRange(workOrder.PackingUnits);
        await _context.SaveChangesAsync();

        var companyPrefix = workOrder.Product.Customer.GLN[..7];
        var ssccGenerator = new SSCCGenerator(companyPrefix, 1);

        // Agrege edilmemiş seri numaralarını al
        var availableSerials = workOrder.SerialNumbers
            .Where(s => s.PackingUnitId == null)
            .OrderBy(s => s.SeriNo)
            .ToList();

        var boxes = new List<PackingUnit>();
        var pallets = new List<PackingUnit>();

        // Kolileri oluştur
        var boxCount = (int)Math.Ceiling((double)availableSerials.Count / workOrder.KoliKapasitesi);
        for (int i = 0; i < boxCount; i++)
        {
            var box = new PackingUnit
            {
                Id = Guid.NewGuid(),
                WorkOrderId = workOrderId,
                Tip = PackingUnitType.Box,
                SSCC = ssccGenerator.GenerateSSCC()
            };
            boxes.Add(box);

            // Bu koliye ait seri numaralarını ata
            var serialsForBox = availableSerials
                .Skip(i * workOrder.KoliKapasitesi)
                .Take(workOrder.KoliKapasitesi)
                .ToList();

            foreach (var serial in serialsForBox)
            {
                serial.PackingUnitId = box.Id;
                serial.Durum = SerialNumberStatus.Aggregated;
            }
        }

        // Paletleri oluştur
        var palletCount = (int)Math.Ceiling((double)boxes.Count / workOrder.PaletKapasitesi);
        for (int i = 0; i < palletCount; i++)
        {
            var pallet = new PackingUnit
            {
                Id = Guid.NewGuid(),
                WorkOrderId = workOrderId,
                Tip = PackingUnitType.Pallet,
                SSCC = ssccGenerator.GenerateSSCC()
            };
            pallets.Add(pallet);

            // Bu palete ait kolileri ata
            var boxesForPallet = boxes
                .Skip(i * workOrder.PaletKapasitesi)
                .Take(workOrder.PaletKapasitesi)
                .ToList();

            foreach (var box in boxesForPallet)
            {
                box.ParentId = pallet.Id;
            }
        }

        _context.PackingUnits.AddRange(pallets);
        _context.PackingUnits.AddRange(boxes);

        // İş emri durumunu güncelle
        workOrder.Durum = WorkOrderStatus.Completed;

        await _context.SaveChangesAsync();

        return new AggregationResultDto
        {
            WorkOrderId = workOrderId,
            TotalSerials = availableSerials.Count,
            TotalBoxes = boxes.Count,
            TotalPallets = pallets.Count,
            BoxCapacity = workOrder.KoliKapasitesi,
            PalletCapacity = workOrder.PaletKapasitesi
        };
    }

    /// <summary>
    /// Belirli seri numaralarını bir koliye ekler
    /// </summary>
    public async Task<PackingUnitDto> CreateBoxAsync(Guid workOrderId, List<Guid> serialNumberIds)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("Work order not found");

        var serials = await _context.SerialNumbers
            .Where(s => serialNumberIds.Contains(s.Id) && s.WorkOrderId == workOrderId && s.PackingUnitId == null)
            .ToListAsync();

        if (serials.Count != serialNumberIds.Count)
            throw new ArgumentException("Some serial numbers are not available");

        var companyPrefix = workOrder.Product.Customer.GLN[..7];
        var existingBoxCount = await _context.PackingUnits
            .CountAsync(p => p.WorkOrderId == workOrderId && p.Tip == PackingUnitType.Box);

        var ssccGenerator = new SSCCGenerator(companyPrefix, existingBoxCount + 1);

        var box = new PackingUnit
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrderId,
            Tip = PackingUnitType.Box,
            SSCC = ssccGenerator.GenerateSSCC()
        };

        foreach (var serial in serials)
        {
            serial.PackingUnitId = box.Id;
            serial.Durum = SerialNumberStatus.Aggregated;
        }

        _context.PackingUnits.Add(box);
        await _context.SaveChangesAsync();

        return new PackingUnitDto
        {
            Id = box.Id,
            Tip = box.Tip,
            SSCC = box.SSCC,
            ItemCount = serials.Count
        };
    }

    /// <summary>
    /// Belirli kolileri bir palete ekler
    /// </summary>
    public async Task<PackingUnitDto> CreatePalletAsync(Guid workOrderId, List<Guid> boxIds)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("Work order not found");

        var boxes = await _context.PackingUnits
            .Where(p => boxIds.Contains(p.Id) && p.WorkOrderId == workOrderId && p.Tip == PackingUnitType.Box && p.ParentId == null)
            .ToListAsync();

        if (boxes.Count != boxIds.Count)
            throw new ArgumentException("Some boxes are not available");

        var companyPrefix = workOrder.Product.Customer.GLN[..7];
        var existingPalletCount = await _context.PackingUnits
            .CountAsync(p => p.WorkOrderId == workOrderId && p.Tip == PackingUnitType.Pallet);

        var ssccGenerator = new SSCCGenerator(companyPrefix, existingPalletCount + 1000); // Palet SSCC'leri farklı range'den

        var pallet = new PackingUnit
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrderId,
            Tip = PackingUnitType.Pallet,
            SSCC = ssccGenerator.GenerateSSCC()
        };

        foreach (var box in boxes)
        {
            box.ParentId = pallet.Id;
        }

        _context.PackingUnits.Add(pallet);
        await _context.SaveChangesAsync();

        return new PackingUnitDto
        {
            Id = pallet.Id,
            Tip = pallet.Tip,
            SSCC = pallet.SSCC,
            ItemCount = boxes.Count
        };
    }
}

public class AggregationResultDto
{
    public Guid WorkOrderId { get; set; }
    public int TotalSerials { get; set; }
    public int TotalBoxes { get; set; }
    public int TotalPallets { get; set; }
    public int BoxCapacity { get; set; }
    public int PalletCapacity { get; set; }
}
