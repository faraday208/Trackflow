using System.Net.Http.Json;
using System.Text.Json;
using Trackflow.Shared.DTOs;

namespace Trackflow.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl = "http://localhost:5101")
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public string BaseUrl => _httpClient.BaseAddress?.ToString() ?? "";

    public async Task<HealthReportDto?> TestConnectionAsync(string url)
    {
        using var testClient = new HttpClient { BaseAddress = new Uri(url.TrimEnd('/')) };
        return await testClient.GetFromJsonAsync<HealthReportDto>("/api/health", _jsonOptions);
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

    public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/customers", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
    }

    public async Task<string> GenerateGLNAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<JsonElement>("/api/customers/generate-gln", _jsonOptions);
        return response.GetProperty("gln").GetString() ?? "";
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, CreateCustomerDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/customers/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/customers/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Products
    public async Task<List<ProductDto>> GetProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ProductDto>>("/api/products", _jsonOptions) ?? new();
    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/products", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, CreateProductDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/products/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>(_jsonOptions);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/products/{id}");
        response.EnsureSuccessStatusCode();
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

    public async Task DeleteWorkOrderAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/workorders/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<AggregationResultDto?> AggregateWorkOrderAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"/api/workorders/{id}/aggregate", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AggregationResultDto>(_jsonOptions);
    }

    public async Task ResetWorkOrderAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"/api/workorders/{id}/reset", null);
        response.EnsureSuccessStatusCode();
    }

    // Serialization - Print
    public async Task<PrintBatchResult?> PrintSerialsAsync(Guid workOrderId, int? count = null)
    {
        var url = count.HasValue
            ? $"/api/serialization/{workOrderId}/print/{count.Value}"
            : $"/api/serialization/{workOrderId}/print";
        var response = await _httpClient.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PrintBatchResult>(_jsonOptions);
    }

    // Serialization - Verify
    public async Task<VerifyBatchResult?> VerifySerialsAsync(Guid workOrderId)
    {
        var response = await _httpClient.PostAsync($"/api/serialization/{workOrderId}/verify", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyBatchResult>(_jsonOptions);
    }

    public async Task<VerifyResult?> VerifySingleAsync(Guid serialId)
    {
        var response = await _httpClient.PostAsync($"/api/serialization/{serialId}/verify-single", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyResult>(_jsonOptions);
    }

    public async Task<VerifyResult?> RetryVerifyAsync(Guid serialId)
    {
        var response = await _httpClient.PostAsync($"/api/serialization/{serialId}/retry-verify", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyResult>(_jsonOptions);
    }

    // Serialization - Status
    public async Task<ProductionStatus?> GetProductionStatusAsync(Guid workOrderId)
    {
        return await _httpClient.GetFromJsonAsync<ProductionStatus>($"/api/serialization/{workOrderId}/status", _jsonOptions);
    }

    // PLC
    public async Task<PLCStatus?> StartProductionAsync(Guid workOrderId)
    {
        var response = await _httpClient.PostAsync($"/api/plc/{workOrderId}/start", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PLCStatus>(_jsonOptions);
    }

    public async Task<PLCStatus?> StopProductionAsync(Guid workOrderId)
    {
        var response = await _httpClient.PostAsync($"/api/plc/{workOrderId}/stop", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PLCStatus>(_jsonOptions);
    }

    public async Task<PLCStatus?> GetPLCStatusAsync(Guid workOrderId)
    {
        return await _httpClient.GetFromJsonAsync<PLCStatus>($"/api/plc/{workOrderId}/status", _jsonOptions);
    }
}
