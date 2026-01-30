using Trackflow.Shared.Enums;

namespace Trackflow.Domain.Entities;

public class WorkOrder : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Miktar { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public DateTime SonKullanmaTarihi { get; set; }
    public int SeriBaslangic { get; set; }
    public WorkOrderStatus Durum { get; set; } = WorkOrderStatus.Created;
    public int KoliKapasitesi { get; set; } = 10;
    public int PaletKapasitesi { get; set; } = 10;

    // Navigation
    public Product Product { get; set; } = null!;
    public ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
    public ICollection<PackingUnit> PackingUnits { get; set; } = new List<PackingUnit>();
}
