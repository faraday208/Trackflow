namespace Trackflow.Shared.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string GTIN { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateProductDto
{
    public Guid CustomerId { get; set; }
    public string GTIN { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    public Guid CustomerId { get; set; }
    public string GTIN { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
}
