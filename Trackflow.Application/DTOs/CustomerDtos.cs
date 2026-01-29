namespace Trackflow.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;
    public string GLN { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomerDto
{
    public string FirmaAdi { get; set; } = string.Empty;
    public string GLN { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
}
