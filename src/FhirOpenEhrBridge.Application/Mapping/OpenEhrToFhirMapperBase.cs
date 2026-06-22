using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Validation;

namespace FhirOpenEhrBridge.Application.Mapping;

/// <summary>
/// Base class for openEHR &#8594; FHIR mappers. Mirrors
/// <see cref="FhirToOpenEhrMapperBase{TOpenEhr}"/>: validate the inbound openEHR
/// model, abort on error, and translate exceptions into mapping failures.
/// </summary>
/// <typeparam name="TOpenEhr">The openEHR input type accepted by the mapper.</typeparam>
public abstract class OpenEhrToFhirMapperBase<TOpenEhr> : IOpenEhrToFhirMapper<TOpenEhr>
{
    private readonly IPayloadValidator<TOpenEhr> _validator;

    /// <param name="validator">Validator applied to the inbound openEHR model.</param>
    protected OpenEhrToFhirMapperBase(IPayloadValidator<TOpenEhr> validator)
    {
        _validator = validator;
    }

    /// <inheritdoc />
    public abstract string SupportedArchetypeId { get; }

    /// <inheritdoc />
    public MappingResult<string> Map(TOpenEhr openEhr)
    {
        var validation = _validator.Validate(openEhr);
        if (!validation.IsValid)
        {
            return MappingResult<string>.Fail(validation.Issues);
        }

        try
        {
            return MapCore(openEhr, validation.Issues);
        }
        catch (Exception ex)
        {
            return MappingResult<string>.Fail($"openEHR-to-FHIR mapping failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs the concrete mapping. Called only after validation has passed.
    /// </summary>
    /// <param name="openEhr">The validated openEHR input model.</param>
    /// <param name="carriedIssues">Non-fatal validation issues to carry onto the result.</param>
    protected abstract MappingResult<string> MapCore(
        TOpenEhr openEhr,
        IReadOnlyList<ValidationIssue> carriedIssues);
}
