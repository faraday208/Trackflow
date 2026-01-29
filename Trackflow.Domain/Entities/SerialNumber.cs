using Trackflow.Domain.Enums;

namespace Trackflow.Domain.Entities;

public class SerialNumber : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public Guid? PackingUnitId { get; set; }
    public string SeriNo { get; set; } = string.Empty;
    public SerialNumberStatus Durum { get; set; } = SerialNumberStatus.Generated;
    public string GS1Barkod { get; set; } = string.Empty;

    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;
    public PackingUnit? PackingUnit { get; set; }
}
