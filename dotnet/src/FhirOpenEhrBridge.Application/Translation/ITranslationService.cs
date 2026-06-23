using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;

namespace FhirOpenEhrBridge.Application.Translation;

/// <summary>
/// Application-level facade that selects the appropriate mapper for an inbound
/// payload and runs the validate-then-map pipeline. This is the port the API
/// depends on.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Translates a FHIR resource (JSON) into an openEHR composition, choosing a
    /// registered mapper by the resource's <c>resourceType</c>.
    /// </summary>
    MappingResult<OpenEhrComposition> FhirToOpenEhr(string fhirJson);

    /// <summary>
    /// Translates an openEHR composition into a FHIR payload (JSON), choosing a
    /// registered mapper by the composition's <c>archetypeNodeId</c>.
    /// </summary>
    MappingResult<string> OpenEhrToFhir(OpenEhrComposition composition);
}
