using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trackflow.Infrastructure.Data;
using Trackflow.Shared.DTOs;
using Trackflow.Shared.Enums;

namespace Trackflow.Application.Services;

/// <summary>
/// Yazıcı simülasyon servisi
/// - %98 başarı oranı
/// - 50-150ms rastgele delay
/// </summary>
public class PrinterService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PrinterService> _logger;
    private readonly Random _random = new();
    private const double SuccessRate = 0.98;

    public PrinterService(AppDbContext context, ILogger<PrinterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Tek seri numarası yazdır
    /// </summary>
    public async Task<PrintResult> PrintAsync(Guid serialNumberId)
    {
        var serial = await _context.SerialNumbers
            .FirstOrDefaultAsync(s => s.Id == serialNumberId);

        if (serial == null)
        {
            return new PrintResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                Message = "Seri numarası bulunamadı",
                PrintedAt = DateTime.UtcNow
            };
        }

        if (serial.Durum != SerialNumberStatus.Generated)
        {
            return new PrintResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                Message = $"Seri numarası yazdırılamaz durumda: {serial.Durum}",
                PrintedAt = DateTime.UtcNow
            };
        }

        // Simülasyon: 50-150ms delay
        var delay = _random.Next(50, 151);
        await Task.Delay(delay);

        // Simülasyon: %98 başarı oranı
        var isSuccess = _random.NextDouble() < SuccessRate;

        if (isSuccess)
        {
            serial.Durum = SerialNumberStatus.Printed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seri numarası yazdırıldı: {SeriNo} ({Delay}ms)", serial.SeriNo, delay);

            return new PrintResult
            {
                SerialNumberId = serialNumberId,
                Success = true,
                Message = "Yazdırma başarılı",
                PrintedAt = DateTime.UtcNow
            };
        }

        _logger.LogWarning("Yazıcı hatası (simülasyon): {SeriNo}", serial.SeriNo);

        return new PrintResult
        {
            SerialNumberId = serialNumberId,
            Success = false,
            Message = "Yazıcı hatası (simülasyon)",
            PrintedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// İş emrine ait seri numaralarını toplu yazdır
    /// </summary>
    public async Task<PrintBatchResult> PrintBatchAsync(Guid workOrderId, int? count = null)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("İş emri bulunamadı");

        // Sadece Generated durumundaki serileri al
        var serialsToPrint = workOrder.SerialNumbers
            .Where(s => s.Durum == SerialNumberStatus.Generated)
            .OrderBy(s => s.SeriNo)
            .ToList();

        if (count.HasValue && count.Value > 0)
        {
            serialsToPrint = serialsToPrint.Take(count.Value).ToList();
        }

        var result = new PrintBatchResult
        {
            Total = serialsToPrint.Count,
            Details = new List<PrintResult>()
        };

        _logger.LogInformation("Toplu yazdırma başlatıldı: {WorkOrderId}, {Count} adet", workOrderId, serialsToPrint.Count);

        foreach (var serial in serialsToPrint)
        {
            var printResult = await PrintAsync(serial.Id);
            result.Details.Add(printResult);

            if (printResult.Success)
                result.Printed++;
            else
                result.Failed++;
        }

        _logger.LogInformation("Toplu yazdırma tamamlandı: {Printed} başarılı, {Failed} başarısız",
            result.Printed, result.Failed);

        return result;
    }
}
