namespace FhirOpenEhrBridge.Infrastructure.Options;

/// <summary>Configuration for the external FHIR server adapter.</summary>
public sealed class FhirServerOptions
{
    /// <summary>Configuration section name in <c>appsettings.json</c>.</summary>
    public const string SectionName = "FhirServer";

    /// <summary>Base URL of the FHIR server (e.g. <c>https://server.fire.ly/</c>).</summary>
    public string BaseUrl { get; set; } = "https://server.fire.ly/";

    /// <summary>Optional bearer token used for authenticated servers.</summary>
    public string? BearerToken { get; set; }

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
