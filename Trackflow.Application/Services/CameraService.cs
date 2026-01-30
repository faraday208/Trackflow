using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trackflow.Infrastructure.Data;
using Trackflow.Shared.DTOs;
using Trackflow.Shared.Enums;

namespace Trackflow.Application.Services;

/// <summary>
/// Kamera simülasyon servisi
/// - %95 başarı oranı
/// - 30-100ms rastgele delay
/// - GS1 barkod format kontrolü
/// </summary>
public class CameraService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CameraService> _logger;
    private readonly Random _random = new();
    private const double SuccessRate = 0.95;

    public CameraService(AppDbContext context, ILogger<CameraService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Tek seri numarası doğrula
    /// </summary>
    public async Task<VerifyResult> VerifyAsync(Guid serialNumberId)
    {
        var serial = await _context.SerialNumbers
            .FirstOrDefaultAsync(s => s.Id == serialNumberId);

        if (serial == null)
        {
            return new VerifyResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                Message = "Seri numarası bulunamadı"
            };
        }

        if (serial.Durum != SerialNumberStatus.Printed)
        {
            return new VerifyResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                ExpectedBarcode = serial.GS1Barkod,
                Message = $"Seri numarası doğrulanamaz durumda: {serial.Durum}"
            };
        }

        // Simülasyon: 30-100ms delay
        var delay = _random.Next(30, 101);
        await Task.Delay(delay);

        // GS1 format kontrolü
        var isValidFormat = ValidateGS1Format(serial.GS1Barkod);
        if (!isValidFormat)
        {
            serial.Durum = SerialNumberStatus.Rejected;
            await _context.SaveChangesAsync();

            _logger.LogWarning("GS1 format hatası: {SeriNo}, Barkod: {Barkod}", serial.SeriNo, serial.GS1Barkod);

            return new VerifyResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                ScannedBarcode = serial.GS1Barkod,
                ExpectedBarcode = serial.GS1Barkod,
                Message = "GS1 format hatası"
            };
        }

        // Simülasyon: %95 başarı oranı
        var isSuccess = _random.NextDouble() < SuccessRate;

        if (isSuccess)
        {
            serial.Durum = SerialNumberStatus.Verified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seri numarası doğrulandı: {SeriNo} ({Delay}ms)", serial.SeriNo, delay);

            return new VerifyResult
            {
                SerialNumberId = serialNumberId,
                Success = true,
                ScannedBarcode = serial.GS1Barkod,
                ExpectedBarcode = serial.GS1Barkod,
                Message = "Doğrulama başarılı"
            };
        }

        // Simülasyon: Okuma hatası
        serial.Durum = SerialNumberStatus.Rejected;
        await _context.SaveChangesAsync();

        _logger.LogWarning("Kamera okuma hatası (simülasyon): {SeriNo}", serial.SeriNo);

        return new VerifyResult
        {
            SerialNumberId = serialNumberId,
            Success = false,
            ScannedBarcode = SimulateCorruptedBarcode(serial.GS1Barkod),
            ExpectedBarcode = serial.GS1Barkod,
            Message = "Barkod okunamadı veya hatalı (simülasyon)"
        };
    }

    /// <summary>
    /// İş emrine ait yazdırılmış seri numaralarını toplu doğrula
    /// </summary>
    public async Task<VerifyBatchResult> VerifyBatchAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
            throw new ArgumentException("İş emri bulunamadı");

        // Sadece Printed durumundaki serileri al
        var serialsToVerify = workOrder.SerialNumbers
            .Where(s => s.Durum == SerialNumberStatus.Printed)
            .OrderBy(s => s.SeriNo)
            .ToList();

        var result = new VerifyBatchResult
        {
            Total = serialsToVerify.Count,
            Details = new List<VerifyResult>()
        };

        _logger.LogInformation("Toplu doğrulama başlatıldı: {WorkOrderId}, {Count} adet", workOrderId, serialsToVerify.Count);

        foreach (var serial in serialsToVerify)
        {
            var verifyResult = await VerifyAsync(serial.Id);
            result.Details.Add(verifyResult);

            if (verifyResult.Success)
                result.Verified++;
            else
                result.Rejected++;
        }

        _logger.LogInformation("Toplu doğrulama tamamlandı: {Verified} doğrulandı, {Rejected} reddedildi",
            result.Verified, result.Rejected);

        return result;
    }

    /// <summary>
    /// Reddedilmiş seriyi tekrar doğrula (manuel kontrol)
    /// </summary>
    public async Task<VerifyResult> RetryVerifyAsync(Guid serialNumberId)
    {
        var serial = await _context.SerialNumbers
            .FirstOrDefaultAsync(s => s.Id == serialNumberId);

        if (serial == null)
        {
            return new VerifyResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                Message = "Seri numarası bulunamadı"
            };
        }

        if (serial.Durum != SerialNumberStatus.Rejected)
        {
            return new VerifyResult
            {
                SerialNumberId = serialNumberId,
                Success = false,
                ExpectedBarcode = serial.GS1Barkod,
                Message = $"Sadece reddedilmiş seriler tekrar denenebilir. Mevcut durum: {serial.Durum}"
            };
        }

        // Manuel kontrol: Her zaman başarılı
        serial.Durum = SerialNumberStatus.Verified;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reddedilmiş seri manuel doğrulandı: {SeriNo}", serial.SeriNo);

        return new VerifyResult
        {
            SerialNumberId = serialNumberId,
            Success = true,
            ScannedBarcode = serial.GS1Barkod,
            ExpectedBarcode = serial.GS1Barkod,
            Message = "Manuel doğrulama başarılı"
        };
    }

    /// <summary>
    /// GS1 barkod formatını doğrula
    /// </summary>
    private static bool ValidateGS1Format(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        // GS1-128 formatı: (01)GTIN(17)EXPIRY(10)LOT(21)SERIAL
        // AI kodlarını kontrol et
        return barcode.Contains("(01)") && barcode.Contains("(21)");
    }

    /// <summary>
    /// Simülasyon için bozuk barkod üret
    /// </summary>
    private string SimulateCorruptedBarcode(string original)
    {
        if (string.IsNullOrEmpty(original) || original.Length < 5)
            return "???";

        var chars = original.ToCharArray();
        var pos = _random.Next(0, chars.Length);
        chars[pos] = '?';
        return new string(chars);
    }
}
