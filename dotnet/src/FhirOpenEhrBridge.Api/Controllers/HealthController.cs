using Microsoft.AspNetCore.Mvc;

namespace FhirOpenEhrBridge.Api.Controllers;

/// <summary>Liveness endpoint used by container orchestrators and CI smoke tests.</summary>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Returns a simple health status payload.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "Healthy", service = "FHIR-OpenEHR-Bridge" });
}
