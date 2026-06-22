namespace FhirOpenEhrBridge.Domain.Validation;

/// <summary>
/// Aggregated outcome of validating a payload. Immutable; construct via the
/// <see cref="Success"/> factory or by passing a collection of issues.
/// </summary>
public sealed class ValidationResult
{
    private readonly List<ValidationIssue> _issues;

    /// <summary>Creates a result from an existing set of issues.</summary>
    public ValidationResult(IEnumerable<ValidationIssue> issues)
    {
        _issues = issues?.ToList() ?? new List<ValidationIssue>();
    }

    private ValidationResult() => _issues = new List<ValidationIssue>();

    /// <summary>All issues recorded during validation.</summary>
    public IReadOnlyList<ValidationIssue> Issues => _issues;

    /// <summary>
    /// <c>true</c> when no <see cref="ValidationSeverity.Error"/> issues are present.
    /// Warnings and information notes do not make a result invalid.
    /// </summary>
    public bool IsValid => _issues.All(i => i.Severity != ValidationSeverity.Error);

    /// <summary>A reusable, valid result with no issues.</summary>
    public static ValidationResult Success { get; } = new();
}
