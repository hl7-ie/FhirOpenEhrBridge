namespace FhirOpenEhrBridge.Domain.Mapping;

/// <summary>
/// Non-generic marker for a mapper that translates a FHIR resource into an
/// openEHR structure. Used for registration and discovery without needing to
/// know the concrete output type.
/// </summary>
public interface IFhirToOpenEhrMapper
{
    /// <summary>
    /// The FHIR resource type this mapper accepts (e.g. <c>Patient</c>), matching
    /// the <c>resourceType</c> field of the incoming JSON.
    /// </summary>
    string SupportedFhirResourceType { get; }
}

/// <summary>
/// Translates a FHIR resource (supplied as JSON) into a strongly typed openEHR
/// output model.
/// </summary>
/// <typeparam name="TOpenEhr">The openEHR output type produced by this mapper.</typeparam>
public interface IFhirToOpenEhrMapper<TOpenEhr> : IFhirToOpenEhrMapper
{
    /// <summary>Maps the supplied FHIR JSON into <typeparamref name="TOpenEhr"/>.</summary>
    /// <param name="fhirJson">The raw FHIR resource JSON.</param>
    /// <returns>A <see cref="MappingResult{T}"/> with the openEHR output or the errors encountered.</returns>
    MappingResult<TOpenEhr> Map(string fhirJson);
}
