using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Trackflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Tüm servislerin sağlık durumunu kontrol eder (API + Database)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        var report = await _healthCheckService.CheckHealthAsync();
        var dto = MapToDto(report);

        return report.Status == HealthStatus.Healthy
            ? Ok(dto)
            : StatusCode(503, dto);
    }

    /// <summary>
    /// Readiness probe - Uygulama trafiğe hazır mı? (Database bağlantısı dahil)
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness()
    {
        var report = await _healthCheckService.CheckHealthAsync(
            predicate: check => check.Tags.Contains("ready"));

        var dto = MapToDto(report);

        return report.Status == HealthStatus.Healthy
            ? Ok(dto)
            : StatusCode(503, dto);
    }

    /// <summary>
    /// Liveness probe - Uygulama çalışıyor mu? (Sadece API kontrolü)
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveness()
    {
        var report = await _healthCheckService.CheckHealthAsync(
            predicate: check => check.Tags.Contains("api"));

        return Ok(MapToDto(report));
    }

    /// <summary>
    /// Database bağlantı durumunu kontrol eder
    /// </summary>
    [HttpGet("database")]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        var report = await _healthCheckService.CheckHealthAsync(
            predicate: check => check.Tags.Contains("db"));

        var dto = MapToDto(report);

        return report.Status == HealthStatus.Healthy
            ? Ok(dto)
            : StatusCode(503, dto);
    }

    private static HealthReportDto MapToDto(HealthReport report)
    {
        return new HealthReportDto
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            TotalDuration = $"{report.TotalDuration.TotalMilliseconds:F2}ms",
            Checks = report.Entries.Select(e => new HealthCheckDto
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = $"{e.Value.Duration.TotalMilliseconds:F2}ms",
                Description = e.Value.Description,
                Exception = e.Value.Exception?.Message,
                Tags = e.Value.Tags.ToList()
            }).ToList()
        };
    }
}

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
    public List<string> Tags { get; set; } = new();
}
