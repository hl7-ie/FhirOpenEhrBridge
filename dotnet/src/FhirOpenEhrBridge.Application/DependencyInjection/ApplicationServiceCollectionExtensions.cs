using FhirOpenEhrBridge.Application.Mapping.Patient;
using FhirOpenEhrBridge.Application.Translation;
using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using Microsoft.Extensions.DependencyInjection;

namespace FhirOpenEhrBridge.Application.DependencyInjection;

/// <summary>
/// Registers the Application layer (mappers and the translation service) into a
/// dependency injection container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the FHIR &#8596; openEHR mappers and the <see cref="ITranslationService"/>.
    /// </summary>
    public static IServiceCollection AddBridgeApplication(this IServiceCollection services)
    {
        // The mappers are stateless (the Firely parser/serializer are shared,
        // thread-safe singletons), so they can be registered as singletons.
        services.AddSingleton<PatientToDemographicsMapper>();
        services.AddSingleton<IFhirToOpenEhrMapper>(sp => sp.GetRequiredService<PatientToDemographicsMapper>());
        services.AddSingleton<IFhirToOpenEhrMapper<OpenEhrComposition>>(sp => sp.GetRequiredService<PatientToDemographicsMapper>());

        services.AddSingleton<DemographicsToPatientMapper>();
        services.AddSingleton<IOpenEhrToFhirMapper>(sp => sp.GetRequiredService<DemographicsToPatientMapper>());
        services.AddSingleton<IOpenEhrToFhirMapper<OpenEhrComposition>>(sp => sp.GetRequiredService<DemographicsToPatientMapper>());

        services.AddSingleton<ITranslationService, TranslationService>();

        return services;
    }
}
