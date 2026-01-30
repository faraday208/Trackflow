using Trackflow.Shared.Enums;

namespace Trackflow.Shared.DTOs;

public class WorkOrderDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string GTIN { get; set; } = string.Empty;
    public int Miktar { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public DateTime SonKullanmaTarihi { get; set; }
    public WorkOrderStatus Durum { get; set; }
    public int KoliKapasitesi { get; set; }
    public int PaletKapasitesi { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWorkOrderDto
{
    public Guid ProductId { get; set; }
    public int Miktar { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public DateTime SonKullanmaTarihi { get; set; }
    public int SeriBaslangic { get; set; } = 1;
    public int KoliKapasitesi { get; set; } = 10;
    public int PaletKapasitesi { get; set; } = 10;
}

public class WorkOrderDetailDto
{
    public Guid Id { get; set; }
    public int Miktar { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public DateTime SonKullanmaTarihi { get; set; }
    public WorkOrderStatus Durum { get; set; }
    public int KoliKapasitesi { get; set; }
    public int PaletKapasitesi { get; set; }
    public DateTime CreatedAt { get; set; }

    public ProductDto Product { get; set; } = null!;
    public CustomerDto Customer { get; set; } = null!;
    public List<SerialNumberDto> SerialNumbers { get; set; } = new();
    public List<PackingUnitDto> PackingUnits { get; set; } = new();

    public int TotalSerials { get; set; }
    public int AggregatedSerials { get; set; }
    public int TotalBoxes { get; set; }
    public int TotalPallets { get; set; }
}

public class SerialNumberDto
{
    public Guid Id { get; set; }
    public string SeriNo { get; set; } = string.Empty;
    public SerialNumberStatus Durum { get; set; }
    public string GS1Barkod { get; set; } = string.Empty;
    public Guid? PackingUnitId { get; set; }
}

public class PackingUnitDto
{
    public Guid Id { get; set; }
    public PackingUnitType Tip { get; set; }
    public string SSCC { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public int ItemCount { get; set; }
    public List<PackingUnitDto> Children { get; set; } = new();
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
