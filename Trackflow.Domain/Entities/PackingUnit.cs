using Trackflow.Shared.Enums;

namespace Trackflow.Domain.Entities;

public class PackingUnit : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public Guid? ParentId { get; set; }
    public PackingUnitType Tip { get; set; }
    public string SSCC { get; set; } = string.Empty;

    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;
    public PackingUnit? Parent { get; set; }
    public ICollection<PackingUnit> Children { get; set; } = new List<PackingUnit>();
    public ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
}
