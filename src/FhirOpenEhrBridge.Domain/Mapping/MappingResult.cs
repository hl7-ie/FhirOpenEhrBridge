using FhirOpenEhrBridge.Domain.Validation;

namespace FhirOpenEhrBridge.Domain.Mapping;

/// <summary>
/// The outcome of a mapping operation. Carries either a successfully produced
/// payload or the validation issues that prevented it.
/// </summary>
/// <typeparam name="T">The type of the mapped output.</typeparam>
public sealed class MappingResult<T>
{
    private MappingResult(bool succeeded, T? value, IReadOnlyList<ValidationIssue> issues)
    {
        Succeeded = succeeded;
        Value = value;
        Issues = issues;
    }

    /// <summary>Whether the mapping produced a usable <see cref="Value"/>.</summary>
    public bool Succeeded { get; }

    /// <summary>The mapped output, or <c>default</c> when <see cref="Succeeded"/> is <c>false</c>.</summary>
    public T? Value { get; }

    /// <summary>Issues raised while mapping (validation errors, warnings or notes).</summary>
    public IReadOnlyList<ValidationIssue> Issues { get; }

    /// <summary>Creates a successful result, optionally carrying non-fatal issues.</summary>
    public static MappingResult<T> Ok(T value, IEnumerable<ValidationIssue>? issues = null) =>
        new(true, value, (issues ?? Enumerable.Empty<ValidationIssue>()).ToList());

    /// <summary>Creates a failed result from a set of issues.</summary>
    public static MappingResult<T> Fail(IEnumerable<ValidationIssue> issues) =>
        new(false, default, issues.ToList());

    /// <summary>Creates a failed result from a single error message.</summary>
    public static MappingResult<T> Fail(string message, string? location = null) =>
        Fail(new[] { new ValidationIssue(ValidationSeverity.Error, message, location) });
}
