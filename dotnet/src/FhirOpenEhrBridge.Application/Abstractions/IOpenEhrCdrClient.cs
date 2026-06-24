namespace FhirOpenEhrBridge.Application.Abstractions;

/// <summary>
/// Outbound port for talking to an external openEHR Clinical Data Repository
/// (CDR), such as EHRbase. Implemented by an adapter in the Infrastructure layer.
/// </summary>
public interface IOpenEhrCdrClient
{
    /// <summary>Creates a new EHR in the CDR and returns its EHR id.</summary>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    Task<string> CreateEhrAsync(CancellationToken cancellationToken = default);

    /// <summary>Stores a composition (canonical openEHR JSON) under an EHR.</summary>
    /// <param name="ehrId">The target EHR id.</param>
    /// <param name="compositionJson">The canonical openEHR composition JSON.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The version uid assigned by the CDR.</returns>
    Task<string> StoreCompositionAsync(string ehrId, string compositionJson, CancellationToken cancellationToken = default);

    /// <summary>Executes an AQL query against the CDR and returns the raw result set JSON.</summary>
    /// <param name="aql">The AQL query string.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    Task<string> ExecuteAqlAsync(string aql, CancellationToken cancellationToken = default);
}
