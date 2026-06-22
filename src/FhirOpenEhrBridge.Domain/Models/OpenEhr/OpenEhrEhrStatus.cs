namespace FhirOpenEhrBridge.Domain.Models.OpenEhr;

/// <summary>
/// Simplified representation of an openEHR <c>EHR_STATUS</c> object. The
/// <c>EHR_STATUS</c> links an EHR to the demographic subject it belongs to and
/// declares the queryable/modifiable flags for that EHR.
/// </summary>
public sealed class OpenEhrEhrStatus
{
    /// <summary>
    /// External identifier of the subject (patient) this EHR belongs to. This is
    /// the <c>subject.external_ref.id.value</c> in the openEHR Reference Model and
    /// is mapped from the FHIR <c>Patient.id</c>.
    /// </summary>
    public string? SubjectId { get; set; }

    /// <summary>The namespace of the subject reference (defaults to <c>DEMOGRAPHIC</c>).</summary>
    public string SubjectNamespace { get; set; } = "DEMOGRAPHIC";

    /// <summary>Whether the EHR is exposed to population queries.</summary>
    public bool IsQueryable { get; set; } = true;

    /// <summary>Whether the EHR may be modified.</summary>
    public bool IsModifiable { get; set; } = true;
}
