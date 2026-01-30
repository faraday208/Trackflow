using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Trackflow.Application.GS1;
using Trackflow.Application.Services;
using Trackflow.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<WorkOrderService>();
builder.Services.AddScoped<AggregationService>();
builder.Services.AddScoped<GS1Service>();
builder.Services.AddScoped<PrinterService>();
builder.Services.AddScoped<CameraService>();
builder.Services.AddScoped<PLCService>();

// Controllers
builder.Services.AddControllers();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "sql", "ready"])
    .AddCheck("api", () => HealthCheckResult.Healthy("API is running"), tags: ["api", "ready"]);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Trackflow API",
        Version = "v1",
        Description = "GS1 Serilizasyon ve Agregasyon Sistemi API"
    });
});

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Trackflow API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

// Health check endpoints
var healthCheckOptions = new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds + "ms",
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
                tags = e.Value.Tags
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
};

// /health - tüm kontroller
app.MapHealthChecks("/health", healthCheckOptions);

// /health/ready - sadece "ready" tag'li kontroller (DB dahil)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = healthCheckOptions.ResponseWriter
});

// /health/live - sadece API (liveness probe)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api"),
    ResponseWriter = healthCheckOptions.ResponseWriter
});

app.Run();
