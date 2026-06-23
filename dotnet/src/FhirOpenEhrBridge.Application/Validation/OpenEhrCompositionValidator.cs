using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using FhirOpenEhrBridge.Domain.Validation;

namespace FhirOpenEhrBridge.Application.Validation;

/// <summary>
/// Validates the structure of an <see cref="OpenEhrComposition"/> before it is
/// handed to a mapper. This is the inbound guard of the openEHR &#8594; FHIR
/// translation pipeline.
/// </summary>
public sealed class OpenEhrCompositionValidator : IPayloadValidator<OpenEhrComposition>
{
    /// <inheritdoc />
    public ValidationResult Validate(OpenEhrComposition payload)
    {
        var issues = new List<ValidationIssue>();

        if (payload is null)
        {
            issues.Add(new ValidationIssue(ValidationSeverity.Error, "openEHR composition is null."));
            return new ValidationResult(issues);
        }

        if (string.IsNullOrWhiteSpace(payload.ArchetypeNodeId))
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "Composition is missing 'archetypeNodeId'.",
                nameof(OpenEhrComposition.ArchetypeNodeId)));
        }

        if (payload.Demographics is null)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "Composition does not contain a demographics payload.",
                nameof(OpenEhrComposition.Demographics)));
        }
        else if (string.IsNullOrWhiteSpace(payload.Demographics.FamilyName) &&
                 string.IsNullOrWhiteSpace(payload.Demographics.GivenName))
        {
            // A demographic record with no name at all is almost certainly an error,
            // but we surface it as a warning so partial records can still flow through.
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Demographics contain neither a family name nor a given name.",
                $"{nameof(OpenEhrComposition.Demographics)}.{nameof(OpenEhrDemographics.FamilyName)}"));
        }

        if (payload.EhrStatus is null || string.IsNullOrWhiteSpace(payload.EhrStatus.SubjectId))
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "EHR_STATUS has no subject id; the produced FHIR Patient will be assigned a generated id.",
                $"{nameof(OpenEhrComposition.EhrStatus)}.{nameof(OpenEhrEhrStatus.SubjectId)}"));
        }

        return new ValidationResult(issues);
    }
}
