namespace Trackflow.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirmaAdi { get; set; } = string.Empty;
    public string GLN { get; set; } = string.Empty;
    public string? Aciklama { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
