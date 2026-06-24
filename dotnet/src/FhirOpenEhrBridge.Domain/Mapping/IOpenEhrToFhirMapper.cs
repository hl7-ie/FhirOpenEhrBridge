namespace FhirOpenEhrBridge.Domain.Mapping;

/// <summary>
/// Non-generic marker for a mapper that translates an openEHR structure into a
/// FHIR payload. Used for registration and discovery without needing to know
/// the concrete input type.
/// </summary>
public interface IOpenEhrToFhirMapper
{
    /// <summary>
    /// The openEHR archetype id this mapper accepts
    /// (e.g. <c>openEHR-EHR-COMPOSITION.demographics.v1</c>).
    /// </summary>
    string SupportedArchetypeId { get; }
}

/// <summary>
/// Translates a strongly typed openEHR input model into a FHIR payload
/// (returned as JSON — typically a <c>Bundle</c> or single resource).
/// </summary>
/// <typeparam name="TOpenEhr">The openEHR input type accepted by this mapper.</typeparam>
public interface IOpenEhrToFhirMapper<TOpenEhr> : IOpenEhrToFhirMapper
{
    /// <summary>Maps the supplied openEHR model into FHIR JSON.</summary>
    /// <param name="openEhr">The openEHR input model.</param>
    /// <returns>A <see cref="MappingResult{T}"/> carrying the FHIR JSON or the errors encountered.</returns>
    MappingResult<string> Map(TOpenEhr openEhr);
}
