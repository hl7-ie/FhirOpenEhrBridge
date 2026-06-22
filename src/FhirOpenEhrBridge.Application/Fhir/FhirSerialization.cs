using Hl7.Fhir.Serialization;

namespace FhirOpenEhrBridge.Application.Fhir;

/// <summary>
/// Shared, reusable Firely SDK parser/serializer instances. The Firely
/// <see cref="FhirJsonParser"/> and <see cref="FhirJsonSerializer"/> are designed
/// to be created once and reused, so they are exposed here as singletons.
/// </summary>
public static class FhirSerialization
{
    /// <summary>A permissive JSON parser used to read incoming FHIR payloads.</summary>
    public static FhirJsonParser Parser { get; } = new(new ParserSettings
    {
        AcceptUnknownMembers = true,
        AllowUnrecognizedEnums = true,
        PermissiveParsing = true
    });

    /// <summary>A compact JSON serializer used to render produced FHIR payloads.</summary>
    public static FhirJsonSerializer Serializer { get; } = new(new SerializerSettings
    {
        Pretty = false,
        AppendNewLine = false
    });
}
