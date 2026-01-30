namespace Trackflow.Shared.DTOs;

public class HealthReportDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TotalDuration { get; set; } = string.Empty;
    public List<HealthCheckDto> Checks { get; set; } = new();
}

public class HealthCheckDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Exception { get; set; }
}
