using System.Net.Http.Headers;
using System.Text;
using FhirOpenEhrBridge.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace FhirOpenEhrBridge.Infrastructure.Clients;

/// <summary>
/// HTTP adapter for an external openEHR CDR exposing the openEHR REST API
/// (verified against EHRbase). Registered as a typed <see cref="HttpClient"/>.
/// </summary>
public sealed class OpenEhrCdrClient : IOpenEhrCdrClient
{
    private const string OpenEhrJsonMediaType = "application/json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenEhrCdrClient> _logger;

    /// <summary>Creates the client over a configured <see cref="HttpClient"/>.</summary>
    public OpenEhrCdrClient(HttpClient httpClient, ILogger<OpenEhrCdrClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> CreateEhrAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "ehr");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(OpenEhrJsonMediaType));
        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // EHRbase returns the new EHR id in the ETag/Location header as well as the body.
        var ehrId = response.Headers.ETag?.Tag?.Trim('"');
        if (!string.IsNullOrWhiteSpace(ehrId))
        {
            return ehrId;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> StoreCompositionAsync(string ehrId, string compositionJson, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"ehr/{ehrId}/composition")
        {
            Content = new StringContent(compositionJson, Encoding.UTF8, OpenEhrJsonMediaType)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(OpenEhrJsonMediaType));
        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var versionUid = response.Headers.ETag?.Tag?.Trim('"');
        return string.IsNullOrWhiteSpace(versionUid)
            ? await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
            : versionUid;
    }

    /// <inheritdoc />
    public async Task<string> ExecuteAqlAsync(string aql, CancellationToken cancellationToken = default)
    {
        // The openEHR REST API exposes AQL under the management ("query/aql") path,
        // which sits one level above the openehr/v1 base, so it is addressed relative
        // to the configured base URL.
        var payload = $"{{\"q\":{System.Text.Json.JsonSerializer.Serialize(aql)}}}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "query/aql")
        {
            Content = new StringContent(payload, Encoding.UTF8, OpenEhrJsonMediaType)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(OpenEhrJsonMediaType));

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}
