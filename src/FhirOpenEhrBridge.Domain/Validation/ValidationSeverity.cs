namespace FhirOpenEhrBridge.Domain.Validation;

/// <summary>Severity of a single <see cref="ValidationIssue"/>.</summary>
public enum ValidationSeverity
{
    /// <summary>Informational note; does not prevent mapping.</summary>
    Information,

    /// <summary>A non-fatal concern; mapping may proceed with possible data loss.</summary>
    Warning,

    /// <summary>A fatal problem; the payload must not be mapped.</summary>
    Error
}
