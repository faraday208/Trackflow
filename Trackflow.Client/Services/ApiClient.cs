using System.Net.Http.Json;
using System.Text.Json;

namespace Trackflow.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl = "http://localhost:5000")
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    // Health
    public async Task<HealthReportDto?> GetHealthAsync()
    {
        return await _httpClient.GetFromJsonAsync<HealthReportDto>("/api/health", _jsonOptions);
    }

    // Customers
    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CustomerDto>>("/api/customers", _jsonOptions) ?? new();
    }

    // Products
    public async Task<List<ProductDto>> GetProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ProductDto>>("/api/products", _jsonOptions) ?? new();
    }

    // Work Orders
    public async Task<List<WorkOrderDto>> GetWorkOrdersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<WorkOrderDto>>("/api/workorders", _jsonOptions) ?? new();
    }

    public async Task<WorkOrderDetailDto?> GetWorkOrderDetailAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<WorkOrderDetailDto>($"/api/workorders/{id}/detail", _jsonOptions);
    }

    public async Task<WorkOrderDto?> CreateWorkOrderAsync(CreateWorkOrderDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/workorders", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WorkOrderDto>(_jsonOptions);
    }

    public async Task<AggregationResultDto?> AggregateWorkOrderAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"/api/workorders/{id}/aggregate", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AggregationResultDto>(_jsonOptions);
    }
}

// DTOs
public class HealthReportDto
{
    public string Status { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string TotalDuration { get; set; } = "";
    public List<HealthCheckDto> Checks { get; set; } = new();
}

public class HealthCheckDto
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string Duration { get; set; } = "";
    public string? Description { get; set; }
    public string? Exception { get; set; }
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FirmaAdi { get; set; } = "";
    public string GLN { get; set; } = "";
    public string? Aciklama { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string GTIN { get; set; } = "";
    public string UrunAdi { get; set; } = "";
}

public class WorkOrderDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string GTIN { get; set; } = "";
    public int Miktar { get; set; }
    public string LotNo { get; set; } = "";
    public DateTime SonKullanmaTarihi { get; set; }
    public int Durum { get; set; }
    public int KoliKapasitesi { get; set; }
    public int PaletKapasitesi { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkOrderDetailDto
{
    public Guid Id { get; set; }
    public int Miktar { get; set; }
    public string LotNo { get; set; } = "";
    public DateTime SonKullanmaTarihi { get; set; }
    public int Durum { get; set; }
    public int KoliKapasitesi { get; set; }
    public int PaletKapasitesi { get; set; }
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
    public string SeriNo { get; set; } = "";
    public int Durum { get; set; }
    public string GS1Barkod { get; set; } = "";
    public Guid? PackingUnitId { get; set; }
}

public class PackingUnitDto
{
    public Guid Id { get; set; }
    public int Tip { get; set; }
    public string SSCC { get; set; } = "";
    public Guid? ParentId { get; set; }
    public int ItemCount { get; set; }
    public List<PackingUnitDto> Children { get; set; } = new();
}

public class CreateWorkOrderDto
{
    public Guid ProductId { get; set; }
    public int Miktar { get; set; }
    public string LotNo { get; set; } = "";
    public DateTime SonKullanmaTarihi { get; set; }
    public int SeriBaslangic { get; set; } = 1;
    public int KoliKapasitesi { get; set; } = 10;
    public int PaletKapasitesi { get; set; } = 10;
}

public class AggregationResultDto
{
    public Guid WorkOrderId { get; set; }
    public int TotalSerials { get; set; }
    public int TotalBoxes { get; set; }
    public int TotalPallets { get; set; }
}
