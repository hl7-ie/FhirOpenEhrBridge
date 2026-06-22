namespace FhirOpenEhrBridge.Domain.Models.OpenEhr;

/// <summary>
/// Simplified representation of an openEHR address cluster
/// (archetype <c>openEHR-DEMOGRAPHIC-ADDRESS.address.v1</c>).
/// </summary>
public sealed class OpenEhrAddress
{
    /// <summary>Street address line.</summary>
    public string? Line { get; set; }

    /// <summary>City / town / locality.</summary>
    public string? City { get; set; }

    /// <summary>Postal code / ZIP code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Country (ISO-3166 name or code).</summary>
    public string? Country { get; set; }
}
