using System.Net.Http.Headers;
using System.Text;
using FhirOpenEhrBridge.Application.Abstractions;
using FhirOpenEhrBridge.Infrastructure.Clients;
using FhirOpenEhrBridge.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FhirOpenEhrBridge.Infrastructure.DependencyInjection;

/// <summary>
/// Registers the Infrastructure layer (external FHIR/openEHR adapters) into a
/// dependency injection container.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Binds the FHIR/CDR options from configuration and registers the typed
    /// <see cref="HttpClient"/> adapters that fulfil the Application ports.
    /// </summary>
    public static IServiceCollection AddBridgeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FhirServerOptions>()
            .Bind(configuration.GetSection(FhirServerOptions.SectionName));

        services.AddOptions<OpenEhrCdrOptions>()
            .Bind(configuration.GetSection(OpenEhrCdrOptions.SectionName));

        services.AddHttpClient<IFhirServerClient, FhirServerClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FhirServerOptions>>().Value;
            client.BaseAddress = new Uri(EnsureTrailingSlash(options.BaseUrl));
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            if (!string.IsNullOrWhiteSpace(options.BearerToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.BearerToken);
            }
        });

        services.AddHttpClient<IOpenEhrCdrClient, OpenEhrCdrClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenEhrCdrOptions>>().Value;
            client.BaseAddress = new Uri(EnsureTrailingSlash(options.BaseUrl));
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                var raw = $"{options.Username}:{options.Password}";
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }
        });

        return services;
    }

    private static string EnsureTrailingSlash(string url) =>
        url.EndsWith('/') ? url : url + "/";
}
