using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Validation;

namespace FhirOpenEhrBridge.Application.Mapping;

/// <summary>
/// Base class for FHIR &#8594; openEHR mappers. It enforces the
/// validate-then-map pipeline: the inbound payload is validated, mapping is
/// aborted on any error, and exceptions thrown by the concrete mapper are
/// converted into mapping failures rather than propagating.
/// </summary>
/// <typeparam name="TOpenEhr">The openEHR output type produced by the mapper.</typeparam>
public abstract class FhirToOpenEhrMapperBase<TOpenEhr> : IFhirToOpenEhrMapper<TOpenEhr>
{
    private readonly IPayloadValidator<string> _validator;

    /// <param name="validator">Validator applied to the inbound FHIR JSON.</param>
    protected FhirToOpenEhrMapperBase(IPayloadValidator<string> validator)
    {
        _validator = validator;
    }

    /// <inheritdoc />
    public abstract string SupportedFhirResourceType { get; }

    /// <inheritdoc />
    public MappingResult<TOpenEhr> Map(string fhirJson)
    {
        var validation = _validator.Validate(fhirJson);
        if (!validation.IsValid)
        {
            return MappingResult<TOpenEhr>.Fail(validation.Issues);
        }

        try
        {
            // Pass through any non-fatal validation issues (warnings/info) so they
            // surface on the successful mapping result.
            return MapCore(fhirJson, validation.Issues);
        }
        catch (Exception ex)
        {
            return MappingResult<TOpenEhr>.Fail($"FHIR-to-openEHR mapping failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs the concrete mapping. Called only after validation has passed.
    /// </summary>
    /// <param name="fhirJson">The validated FHIR resource JSON.</param>
    /// <param name="carriedIssues">Non-fatal validation issues to carry onto the result.</param>
    protected abstract MappingResult<TOpenEhr> MapCore(
        string fhirJson,
        IReadOnlyList<ValidationIssue> carriedIssues);
}
