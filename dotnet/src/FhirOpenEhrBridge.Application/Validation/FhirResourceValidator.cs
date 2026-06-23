using FhirOpenEhrBridge.Domain.Validation;
using FhirOpenEhrBridge.Application.Fhir;
using Hl7.Fhir.Model;

namespace FhirOpenEhrBridge.Application.Validation;

/// <summary>
/// Validates that an incoming string is well-formed FHIR JSON of the expected
/// resource type before it is handed to a mapper. This is the inbound guard of
/// the FHIR &#8594; openEHR translation pipeline.
/// </summary>
public sealed class FhirResourceValidator : IPayloadValidator<string>
{
    private readonly string _expectedResourceType;

    /// <summary>
    /// Creates a validator for a specific FHIR resource type.
    /// </summary>
    /// <param name="expectedResourceType">
    /// The <c>resourceType</c> the payload must declare (e.g. <c>Patient</c>).
    /// </param>
    public FhirResourceValidator(string expectedResourceType)
    {
        _expectedResourceType = expectedResourceType;
    }

    /// <inheritdoc />
    public ValidationResult Validate(string payload)
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(payload))
        {
            issues.Add(new ValidationIssue(ValidationSeverity.Error, "FHIR payload is empty."));
            return new ValidationResult(issues);
        }

        Resource? resource;
        try
        {
            resource = FhirSerialization.Parser.Parse<Resource>(payload);
        }
        catch (Exception ex)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                $"Payload is not valid FHIR JSON: {ex.Message}"));
            return new ValidationResult(issues);
        }

        if (resource is null)
        {
            issues.Add(new ValidationIssue(ValidationSeverity.Error, "Payload did not parse to a FHIR resource."));
            return new ValidationResult(issues);
        }

        if (!string.Equals(resource.TypeName, _expectedResourceType, StringComparison.Ordinal))
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                $"Expected a FHIR '{_expectedResourceType}' resource but received '{resource.TypeName}'.",
                "resourceType"));
        }

        return new ValidationResult(issues);
    }
}
