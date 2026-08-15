using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Api.Controllers;

/// <summary>Shape of the health-check response.</summary>
public record HealthResponse(string Status, DateTime Timestamp);

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check requested");
        return Ok(new HealthResponse("ok", DateTime.UtcNow));
    }
}
