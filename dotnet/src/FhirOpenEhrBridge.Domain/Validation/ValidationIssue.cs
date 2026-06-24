namespace FhirOpenEhrBridge.Domain.Validation;

/// <summary>A single problem discovered while validating a payload.</summary>
/// <param name="Severity">How serious the issue is.</param>
/// <param name="Message">Human readable description of the issue.</param>
/// <param name="Location">
/// Optional dotted path to the offending element (e.g. <c>Patient.name[0].family</c>).
/// </param>
public sealed record ValidationIssue(
    ValidationSeverity Severity,
    string Message,
    string? Location = null);
