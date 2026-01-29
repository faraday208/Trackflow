namespace Trackflow.Domain.Entities;

public class Product : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string GTIN { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
