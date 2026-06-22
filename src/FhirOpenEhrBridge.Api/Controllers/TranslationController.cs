using System.Text;
using System.Text.Json.Nodes;
using FhirOpenEhrBridge.Api.Models;
using FhirOpenEhrBridge.Application.Translation;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using Microsoft.AspNetCore.Mvc;

namespace FhirOpenEhrBridge.Api.Controllers;

/// <summary>
/// Endpoints for bidirectional translation between FHIR and openEHR payloads.
/// </summary>
[ApiController]
[Route("api/translate")]
[Produces("application/json")]
public sealed class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;

    /// <summary>Creates the controller with the translation facade.</summary>
    public TranslationController(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    /// <summary>
    /// Translates a FHIR resource (raw JSON in the request body) into an openEHR
    /// demographics composition.
    /// </summary>
    /// <remarks>
    /// The request body is the raw FHIR resource JSON (e.g. a <c>Patient</c>).
    /// Accepts both <c>application/json</c> and <c>application/fhir+json</c>.
    /// </remarks>
    [HttpPost("fhir-to-openehr")]
    [ProducesResponseType(typeof(TranslationResponse<OpenEhrComposition>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TranslationResponse<OpenEhrComposition>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FhirToOpenEhr()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var fhirJson = await reader.ReadToEndAsync();

        var result = _translationService.FhirToOpenEhr(fhirJson);
        var response = new TranslationResponse<OpenEhrComposition>(result.Succeeded, result.Value, result.Issues);

        return result.Succeeded ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Translates an openEHR demographics composition into a FHIR <c>Bundle</c>.
    /// </summary>
    /// <param name="composition">The openEHR composition to translate.</param>
    [HttpPost("openehr-to-fhir")]
    [ProducesResponseType(typeof(TranslationResponse<JsonNode>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TranslationResponse<JsonNode>), StatusCodes.Status400BadRequest)]
    public IActionResult OpenEhrToFhir([FromBody] OpenEhrComposition composition)
    {
        var result = _translationService.OpenEhrToFhir(composition);
        if (!result.Succeeded)
        {
            return BadRequest(new TranslationResponse<JsonNode>(false, null, result.Issues));
        }

        // Re-parse the FHIR JSON so it is emitted as a real JSON object rather than
        // an escaped string inside the response envelope.
        var bundle = JsonNode.Parse(result.Value!);
        return Ok(new TranslationResponse<JsonNode>(true, bundle, result.Issues));
    }
}
