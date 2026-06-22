namespace FhirOpenEhrBridge.Domain.Models.OpenEhr;

/// <summary>
/// Simplified, JSON-serializable representation of the demographic information
/// carried by an openEHR <c>PERSON</c> party (archetype
/// <c>openEHR-DEMOGRAPHIC-PERSON.person.v1</c>).
/// <para>
/// This is intentionally a plain data model (a "mapping model") rather than a
/// full openEHR Reference Model implementation. It captures the subset of
/// demographic concepts that have a meaningful, lossless mapping to/from a
/// FHIR <c>Patient</c> resource.
/// </para>
/// </summary>
public sealed class OpenEhrDemographics
{
    /// <summary>Family name (surname) of the person.</summary>
    public string? FamilyName { get; set; }

    /// <summary>Given name (forename) of the person.</summary>
    public string? GivenName { get; set; }

    /// <summary>
    /// Administrative gender, expressed using the openEHR local value-set codes
    /// (<c>male</c>, <c>female</c>, <c>intersex</c>, <c>unknown</c>).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>Date of birth in ISO-8601 (<c>yyyy-MM-dd</c>) format.</summary>
    public string? BirthDate { get; set; }

    /// <summary>Business identifiers assigned to the person (e.g. national patient id).</summary>
    public List<OpenEhrIdentifier> Identifiers { get; set; } = new();

    /// <summary>Primary postal address of the person, if known.</summary>
    public OpenEhrAddress? Address { get; set; }
}
