using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FhirOpenEhrBridge.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace FhirOpenEhrBridge.Infrastructure.Clients;

/// <summary>
/// HTTP adapter for an external FHIR server. Registered as a typed
/// <see cref="HttpClient"/>; the base address and auth header are configured in
/// <c>AddBridgeInfrastructure</c>.
/// </summary>
public sealed class FhirServerClient : IFhirServerClient
{
    private const string FhirJsonMediaType = "application/fhir+json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<FhirServerClient> _logger;

    /// <summary>Creates the client over a configured <see cref="HttpClient"/>.</summary>
    public FhirServerClient(HttpClient httpClient, ILogger<FhirServerClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string?> ReadResourceAsync(string resourceType, string id, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{resourceType}/{id}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(FhirJsonMediaType));

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("FHIR {ResourceType}/{Id} was not found.", resourceType, id);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> CreateResourceAsync(string resourceType, string resourceJson, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, resourceType)
        {
            Content = new StringContent(resourceJson, Encoding.UTF8, FhirJsonMediaType)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(FhirJsonMediaType));

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}
