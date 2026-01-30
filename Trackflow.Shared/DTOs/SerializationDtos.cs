namespace Trackflow.Shared.DTOs;

/// <summary>
/// Tek seri numarası yazdırma sonucu
/// </summary>
public class PrintResult
{
    public Guid SerialNumberId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime PrintedAt { get; set; }
}

/// <summary>
/// Toplu yazdırma sonucu
/// </summary>
public class PrintBatchResult
{
    public int Total { get; set; }
    public int Printed { get; set; }
    public int Failed { get; set; }
    public List<PrintResult> Details { get; set; } = new();
}

/// <summary>
/// Tek seri numarası doğrulama sonucu
/// </summary>
public class VerifyResult
{
    public Guid SerialNumberId { get; set; }
    public bool Success { get; set; }
    public string ScannedBarcode { get; set; } = string.Empty;
    public string ExpectedBarcode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Toplu doğrulama sonucu
/// </summary>
public class VerifyBatchResult
{
    public int Total { get; set; }
    public int Verified { get; set; }
    public int Rejected { get; set; }
    public List<VerifyResult> Details { get; set; } = new();
}

/// <summary>
/// Üretim durumu istatistikleri
/// </summary>
public class ProductionStatus
{
    public Guid WorkOrderId { get; set; }
    public int TotalSerials { get; set; }
    public int Generated { get; set; }
    public int Printed { get; set; }
    public int Verified { get; set; }
    public int Rejected { get; set; }
    public int Aggregated { get; set; }
    public bool IsRunning { get; set; }
}

/// <summary>
/// PLC durumu
/// </summary>
public class PLCStatus
{
    public Guid WorkOrderId { get; set; }
    public bool IsRunning { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
