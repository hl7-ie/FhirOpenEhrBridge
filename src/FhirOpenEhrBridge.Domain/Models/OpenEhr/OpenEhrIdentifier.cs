namespace FhirOpenEhrBridge.Domain.Models.OpenEhr;

/// <summary>
/// Represents an openEHR <c>DV_IDENTIFIER</c> data value — a business identifier
/// assigned by an external authority.
/// </summary>
public sealed class OpenEhrIdentifier
{
    /// <summary>The identifier value itself (e.g. the NHS number or MRN).</summary>
    public string? Id { get; set; }

    /// <summary>The organisation or system that issued the identifier (maps to FHIR <c>Identifier.system</c>).</summary>
    public string? Issuer { get; set; }

    /// <summary>The type/category of the identifier (e.g. <c>MRN</c>, <c>NHS</c>).</summary>
    public string? Type { get; set; }
}
