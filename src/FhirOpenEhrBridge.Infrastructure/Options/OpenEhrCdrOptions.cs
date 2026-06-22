namespace FhirOpenEhrBridge.Infrastructure.Options;

/// <summary>Configuration for the external openEHR CDR adapter (e.g. EHRbase).</summary>
public sealed class OpenEhrCdrOptions
{
    /// <summary>Configuration section name in <c>appsettings.json</c>.</summary>
    public const string SectionName = "OpenEhrCdr";

    /// <summary>Base URL of the CDR's openEHR REST API (e.g. <c>http://localhost:8080/ehrbase/rest/openehr/v1/</c>).</summary>
    public string BaseUrl { get; set; } = "http://localhost:8080/ehrbase/rest/openehr/v1/";

    /// <summary>Optional basic-auth user name.</summary>
    public string? Username { get; set; }

    /// <summary>Optional basic-auth password.</summary>
    public string? Password { get; set; }

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
