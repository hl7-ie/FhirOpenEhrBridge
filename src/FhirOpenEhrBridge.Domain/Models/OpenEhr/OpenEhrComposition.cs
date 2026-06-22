namespace FhirOpenEhrBridge.Domain.Models.OpenEhr;

/// <summary>
/// Simplified, JSON-serializable representation of an openEHR <c>COMPOSITION</c>.
/// <para>
/// For the demographics proof-of-concept the composition carries an
/// <see cref="EhrStatus"/> (linking the EHR to its subject) and a
/// <see cref="Demographics"/> payload. The shape mirrors the canonical openEHR
/// JSON well enough to be recognised by downstream openEHR tooling while
/// remaining trivial to construct and assert against in tests.
/// </para>
/// </summary>
public sealed class OpenEhrComposition
{
    /// <summary>The archetype node id of the composition root.</summary>
    public string ArchetypeNodeId { get; set; } = "openEHR-EHR-COMPOSITION.demographics.v1";

    /// <summary>Human readable name of the composition.</summary>
    public string Name { get; set; } = "Demographics";

    /// <summary>ISO-639 language code for the composition content.</summary>
    public string Language { get; set; } = "en";

    /// <summary>ISO-3166 territory code in which the composition was authored.</summary>
    public string Territory { get; set; } = "GB";

    /// <summary>openEHR composition category (<c>event</c>, <c>persistent</c>, …).</summary>
    public string Category { get; set; } = "persistent";

    /// <summary>The clinically/administratively relevant time of the composition.</summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>The EHR_STATUS that links this content to its demographic subject.</summary>
    public OpenEhrEhrStatus EhrStatus { get; set; } = new();

    /// <summary>The demographic content carried by the composition.</summary>
    public OpenEhrDemographics Demographics { get; set; } = new();
}
