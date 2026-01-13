using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;

namespace LoanApplication.API.Controllers;

/// <summary>
/// Health check controller for Kubernetes/OpenShift probes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Liveness probe - checks if the application is running
    /// </summary>
    /// <returns>OK if alive</returns>
    [HttpGet("live")]
    [SwaggerOperation(Summary = "Liveness probe", Description = "Checks if the application is running")]
    [SwaggerResponse(200, "Application is alive")]
    public IActionResult Live()
    {
        return Ok(new 
        { 
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "LoanApplication.API",
            check = "liveness"
        });
    }

    /// <summary>
    /// Readiness probe - checks if the application is ready to receive traffic
    /// </summary>
    /// <returns>OK if ready, Service Unavailable otherwise</returns>
    [HttpGet("ready")]
    [SwaggerOperation(Summary = "Readiness probe", Description = "Checks if the application is ready to receive traffic")]
    [SwaggerResponse(200, "Application is ready")]
    [SwaggerResponse(503, "Application is not ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            var result = await _healthCheckService.CheckHealthAsync(
                predicate: check => check.Tags.Contains("ready"));

            if (result.Status == HealthStatus.Healthy)
            {
                return Ok(new
                {
                    status = result.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    service = "LoanApplication.API",
                    check = "readiness",
                    checks = result.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds
                    })
                });
            }

            return StatusCode(503, new
            {
                status = result.Status.ToString(),
                timestamp = DateTime.UtcNow,
                service = "LoanApplication.API",
                check = "readiness",
                checks = result.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                service = "LoanApplication.API",
                check = "readiness",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Startup probe - checks if the application has started
    /// </summary>
    /// <returns>OK if started</returns>
    [HttpGet("startup")]
    [SwaggerOperation(Summary = "Startup probe", Description = "Checks if the application has started")]
    [SwaggerResponse(200, "Application has started")]
    public IActionResult Startup()
    {
        return Ok(new
        {
            status = "Started",
            timestamp = DateTime.UtcNow,
            service = "LoanApplication.API",
            check = "startup",
            version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0"
        });
    }

    /// <summary>
    /// Detailed health check
    /// </summary>
    /// <returns>Detailed health status</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Detailed health check", Description = "Returns detailed health check information")]
    [SwaggerResponse(200, "Health check completed")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var result = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = result.Status.ToString(),
                timestamp = DateTime.UtcNow,
                service = "LoanApplication.API",
                version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                totalDuration = result.TotalDuration.TotalMilliseconds,
                checks = result.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    data = e.Value.Data.Any() ? e.Value.Data : null,
                    exception = e.Value.Exception?.Message
                })
            };

            return result.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                service = "LoanApplication.API",
                error = ex.Message
            });
        }
    }
}
