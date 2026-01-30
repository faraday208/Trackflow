using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trackflow.Infrastructure.Data;
using Trackflow.Shared.DTOs;
using Trackflow.Shared.Enums;

namespace Trackflow.Application.Services;

/// <summary>
/// PLC simülasyon servisi
/// - Üretim hattı başlat/durdur
/// - Palet tamamlandı sinyali
/// - Loglama ile simülasyon
/// </summary>
public class PLCService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PLCService> _logger;

    // Bellek içi durum takibi (gerçek uygulamada Redis/DB kullanılır)
    private static readonly Dictionary<Guid, PLCStatus> _plcStates = new();
    private static readonly object _lock = new();

    public PLCService(AppDbContext context, ILogger<PLCService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Üretim hattını başlat
    /// </summary>
    public async Task<PLCStatus> StartProductionAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("İş emri bulunamadı");

        lock (_lock)
        {
            if (_plcStates.TryGetValue(workOrderId, out var existingState) && existingState.IsRunning)
            {
                _logger.LogWarning("Üretim zaten çalışıyor: {WorkOrderId}", workOrderId);
                return existingState;
            }

            var status = new PLCStatus
            {
                WorkOrderId = workOrderId,
                IsRunning = true,
                StartedAt = DateTime.UtcNow,
                StoppedAt = null,
                Message = "Üretim hattı başlatıldı"
            };

            _plcStates[workOrderId] = status;

            // İş emri durumunu güncelle
            workOrder.Durum = WorkOrderStatus.InProgress;
            _context.SaveChanges();

            _logger.LogInformation("PLC: Üretim başlatıldı - İş Emri: {WorkOrderId}, Lot: {LotNo}",
                workOrderId, workOrder.LotNo);

            return status;
        }
    }

    /// <summary>
    /// Üretim hattını durdur
    /// </summary>
    public async Task<PLCStatus> StopProductionAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("İş emri bulunamadı");

        lock (_lock)
        {
            if (!_plcStates.TryGetValue(workOrderId, out var existingState) || !existingState.IsRunning)
            {
                _logger.LogWarning("Üretim zaten durmuş: {WorkOrderId}", workOrderId);
                return new PLCStatus
                {
                    WorkOrderId = workOrderId,
                    IsRunning = false,
                    Message = "Üretim zaten durmuş"
                };
            }

            existingState.IsRunning = false;
            existingState.StoppedAt = DateTime.UtcNow;
            existingState.Message = "Üretim hattı durduruldu";

            _logger.LogInformation("PLC: Üretim durduruldu - İş Emri: {WorkOrderId}, Süre: {Duration}",
                workOrderId, existingState.StoppedAt - existingState.StartedAt);

            return existingState;
        }
    }

    /// <summary>
    /// PLC durumunu al
    /// </summary>
    public Task<PLCStatus> GetStatusAsync(Guid workOrderId)
    {
        lock (_lock)
        {
            if (_plcStates.TryGetValue(workOrderId, out var status))
            {
                return Task.FromResult(status);
            }

            return Task.FromResult(new PLCStatus
            {
                WorkOrderId = workOrderId,
                IsRunning = false,
                Message = "Üretim başlatılmamış"
            });
        }
    }

    /// <summary>
    /// Palet tamamlandı sinyali
    /// </summary>
    public async Task<bool> PalletCompleteSignalAsync(Guid palletId)
    {
        var pallet = await _context.PackingUnits
            .Include(p => p.WorkOrder)
            .FirstOrDefaultAsync(p => p.Id == palletId && p.Tip == PackingUnitType.Pallet);

        if (pallet == null)
        {
            _logger.LogWarning("PLC: Palet bulunamadı: {PalletId}", palletId);
            return false;
        }

        _logger.LogInformation("PLC: Palet tamamlandı sinyali - Palet: {PalletId}, SSCC: {SSCC}, İş Emri: {WorkOrderId}",
            palletId, pallet.SSCC, pallet.WorkOrderId);

        // Simülasyon: Sinyal başarıyla gönderildi
        return true;
    }

    /// <summary>
    /// Üretim durumu istatistikleri
    /// </summary>
    public async Task<ProductionStatus> GetProductionStatusAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("İş emri bulunamadı");

        var serials = workOrder.SerialNumbers;
        var isRunning = false;

        lock (_lock)
        {
            if (_plcStates.TryGetValue(workOrderId, out var state))
            {
                isRunning = state.IsRunning;
            }
        }

        return new ProductionStatus
        {
            WorkOrderId = workOrderId,
            TotalSerials = serials.Count,
            Generated = serials.Count(s => s.Durum == SerialNumberStatus.Generated),
            Printed = serials.Count(s => s.Durum == SerialNumberStatus.Printed),
            Verified = serials.Count(s => s.Durum == SerialNumberStatus.Verified),
            Rejected = serials.Count(s => s.Durum == SerialNumberStatus.Rejected),
            Aggregated = serials.Count(s => s.Durum == SerialNumberStatus.Aggregated),
            IsRunning = isRunning
        };
    }
}
