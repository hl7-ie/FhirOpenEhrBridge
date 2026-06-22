using Hl7.Fhir.Model;

namespace FhirOpenEhrBridge.Application.Fhir;

/// <summary>
/// Translates between FHIR <see cref="AdministrativeGender"/> codes and the
/// openEHR local gender value-set used by the demographics mapping models.
/// </summary>
/// <remarks>
/// FHIR <c>other</c> is mapped to openEHR <c>intersex</c> as the closest
/// available concept. This is a documented, lossy assumption of the mapping
/// model (CC-BY) rather than a normative HL7/openEHR equivalence.
/// </remarks>
public static class GenderMap
{
    /// <summary>The openEHR code used when no gender is supplied or recognised.</summary>
    public const string Unknown = "unknown";

    /// <summary>Maps a FHIR administrative gender to an openEHR gender code.</summary>
    public static string ToOpenEhr(AdministrativeGender? gender) => gender switch
    {
        AdministrativeGender.Male => "male",
        AdministrativeGender.Female => "female",
        AdministrativeGender.Other => "intersex",
        _ => Unknown
    };

    /// <summary>Maps an openEHR gender code to a FHIR administrative gender.</summary>
    public static AdministrativeGender? ToFhir(string? openEhrGender) =>
        (openEhrGender?.Trim().ToLowerInvariant()) switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            "intersex" => AdministrativeGender.Other,
            "unknown" => AdministrativeGender.Unknown,
            null or "" => null,
            _ => AdministrativeGender.Unknown
        };
}
