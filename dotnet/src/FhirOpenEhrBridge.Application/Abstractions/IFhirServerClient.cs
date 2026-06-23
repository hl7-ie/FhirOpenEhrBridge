namespace FhirOpenEhrBridge.Application.Abstractions;

/// <summary>
/// Outbound port for talking to an external FHIR server (e.g. a HAPI FHIR or
/// Azure FHIR service). Implemented by an adapter in the Infrastructure layer.
/// </summary>
public interface IFhirServerClient
{
    /// <summary>Reads a resource by type and id, returning the raw FHIR JSON.</summary>
    /// <param name="resourceType">FHIR resource type (e.g. <c>Patient</c>).</param>
    /// <param name="id">Logical id of the resource.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The resource JSON, or <c>null</c> when not found.</returns>
    Task<string?> ReadResourceAsync(string resourceType, string id, CancellationToken cancellationToken = default);

    /// <summary>Creates a resource on the server from raw FHIR JSON.</summary>
    /// <param name="resourceType">FHIR resource type (e.g. <c>Patient</c>).</param>
    /// <param name="resourceJson">The resource JSON to POST.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The server's response JSON (typically the stored resource).</returns>
    Task<string> CreateResourceAsync(string resourceType, string resourceJson, CancellationToken cancellationToken = default);
}
