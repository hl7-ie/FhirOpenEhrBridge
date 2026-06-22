using FhirOpenEhrBridge.Application.Fhir;
using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace FhirOpenEhrBridge.Application.Translation;

/// <summary>
/// Default <see cref="ITranslationService"/>. Maintains a registry of the
/// available mappers and dispatches each request to the mapper that supports
/// the inbound payload's resource type / archetype.
/// </summary>
public sealed class TranslationService : ITranslationService
{
    private readonly IReadOnlyList<IFhirToOpenEhrMapper> _fhirToOpenEhrMappers;
    private readonly IReadOnlyList<IOpenEhrToFhirMapper> _openEhrToFhirMappers;
    private readonly ILogger<TranslationService> _logger;

    /// <summary>
    /// Creates the service from the registered mappers in both directions.
    /// </summary>
    public TranslationService(
        IEnumerable<IFhirToOpenEhrMapper> fhirToOpenEhrMappers,
        IEnumerable<IOpenEhrToFhirMapper> openEhrToFhirMappers,
        ILogger<TranslationService> logger)
    {
        _fhirToOpenEhrMappers = fhirToOpenEhrMappers.ToList();
        _openEhrToFhirMappers = openEhrToFhirMappers.ToList();
        _logger = logger;
    }

    /// <inheritdoc />
    public MappingResult<OpenEhrComposition> FhirToOpenEhr(string fhirJson)
    {
        if (string.IsNullOrWhiteSpace(fhirJson))
        {
            return MappingResult<OpenEhrComposition>.Fail("FHIR payload is empty.");
        }

        string resourceType;
        try
        {
            resourceType = FhirSerialization.Parser.Parse<Resource>(fhirJson).TypeName;
        }
        catch (Exception ex)
        {
            return MappingResult<OpenEhrComposition>.Fail($"Payload is not valid FHIR JSON: {ex.Message}");
        }

        var mapper = _fhirToOpenEhrMappers
            .FirstOrDefault(m => string.Equals(m.SupportedFhirResourceType, resourceType, StringComparison.Ordinal));

        if (mapper is null)
        {
            return MappingResult<OpenEhrComposition>.Fail(
                $"No FHIR-to-openEHR mapper is registered for resource type '{resourceType}'.");
        }

        if (mapper is not IFhirToOpenEhrMapper<OpenEhrComposition> typedMapper)
        {
            return MappingResult<OpenEhrComposition>.Fail(
                $"Mapper for '{resourceType}' does not produce an openEHR composition.");
        }

        _logger.LogInformation("Translating FHIR {ResourceType} to openEHR composition.", resourceType);
        return typedMapper.Map(fhirJson);
    }

    /// <inheritdoc />
    public MappingResult<string> OpenEhrToFhir(OpenEhrComposition composition)
    {
        if (composition is null)
        {
            return MappingResult<string>.Fail("openEHR composition is null.");
        }

        var archetype = composition.ArchetypeNodeId;
        var mapper = _openEhrToFhirMappers
            .FirstOrDefault(m => string.Equals(m.SupportedArchetypeId, archetype, StringComparison.Ordinal));

        if (mapper is null)
        {
            return MappingResult<string>.Fail(
                $"No openEHR-to-FHIR mapper is registered for archetype '{archetype}'.");
        }

        if (mapper is not IOpenEhrToFhirMapper<OpenEhrComposition> typedMapper)
        {
            return MappingResult<string>.Fail(
                $"Mapper for '{archetype}' does not accept an openEHR composition.");
        }

        _logger.LogInformation("Translating openEHR composition {Archetype} to FHIR.", archetype);
        return typedMapper.Map(composition);
    }
}
